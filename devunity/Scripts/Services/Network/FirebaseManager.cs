using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChibitsLink.Repositories;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.Services.Network
{
    /// <summary>
    /// Gestor de conexión y operaciones con Firebase Firestore.
    /// Maneja persistencia de datos de usuarios, partidas y estadísticas en la nube.
    /// Implementa patrón Singleton para acceso global a la base de datos.
    /// </summary>
    /// <remarks>
    /// Este servicio requiere configuración previa de Firebase en el proyecto Unity.
    /// Todas las operaciones son asíncronas para no bloquear el hilo principal.
    /// </remarks>
    /// <seealso href="https://firebase.google.com/docs/firestore">Firebase Firestore Documentation</seealso>
    public class FirebaseManager : MonoBehaviour
    {
        /// <summary>Instancia global del gestor Firebase (patrón Singleton)</summary>
        public static FirebaseManager Instance;
        /// <summary>Instancia de la base de datos Firestore para operaciones CRUD</summary>
        private FirebaseFirestore _database;
        /// <summary>Indica si Firebase ha sido inicializado correctamente</summary>
        private bool _isInitialized = false;
        
        // Repositorios
        private PartyRepository _partyRepository;
        private UserRepository _userRepository;
        private SessionRepository _sessionRepository;

        /// <summary>
        /// Inicializa el gestor Firebase y establece el patrón Singleton.
        /// Configura automáticamente las dependencias de Firebase y repositories.
        /// </summary>
        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
            }
            else 
            {
                Destroy(gameObject);
            }

            InitializeFirebase();
        }

        /// <summary>
        /// Inicializa las dependencias de Firebase y crea la instancia de Firestore.
        /// Configura los repositories para acceso a datos una vez que Firebase está listo.
        /// </summary>
        /// <remarks>
        /// Este método se ejecuta asíncronamente usando ContinueWithOnMainThread
        /// para asegurar que las operaciones de UI se realicen en el hilo principal.
        /// </remarks>
        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _database = FirebaseFirestore.DefaultInstance;
                    _isInitialized = true;
                    
                    // Initialize repositories
                    _partyRepository = new PartyRepository(_database);
                    _userRepository = new UserRepository(_database);
                    _sessionRepository = new SessionRepository(_database);
                    
                    Debug.Log("[FirebaseManager] Firebase Firestore and repositories initialized successfully.");
                }
                else
                {
                    Debug.LogError($"[FirebaseManager] Could not initialize Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        /// <summary>
        /// Actualiza los datos de una sesión de juego activa en Firebase.
        /// Registra información del host, cantidad de jugadores y puntos actuales.
        /// </summary>
        /// <param name="host">Nombre o ID del host de la partida</param>
        /// <param name="players">Cantidad de jugadores actualmente conectados</param>
        /// <param name="points">Puntos actuales de la sesión</param>
        /// <remarks>
        /// Este método es asíncrono pero se llama como void para compatibilidad con Unity.
        /// Los errores se registran en la consola pero no lanzan excepciones.
        /// </remarks>
        public async void UpdateGameSession(string host, int players, int points)
        {
            if (_isInitialized)
            {
                try
                {
                    await _sessionRepository.UpdateGameSessionAsync(host, players, points);
                    Debug.Log($"[FirebaseManager] Game session updated for host {host}");
                }
                catch (RepositoryException ex)
                {
                    Debug.LogError($"[FirebaseManager] Failed to update game session: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError("[FirebaseManager] Not initialized");
            }
        }

        /// <summary>
        /// Finaliza y procesa las puntuaciones de una partida completada.
        /// Actualiza estadísticas de jugadores e historial de partidas en Firebase.
        /// </summary>
        /// <param name="roomCode">Código único de la sala/partida</param>
        /// <param name="finalScores">Diccionario con puntuaciones finales por jugador</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        /// <exception cref="UserException">Error en operaciones de usuario</exception>
        /// <exception cref="RepositoryException">Error en operaciones de base de datos</exception>
        /// <example>
        /// <code>
        /// var scores = new Dictionary&lt;string, int&gt; { {"player1", 100}, {"player2", 85} };
        /// await firebaseManager.FinalizePartyScoresAsync("ROOM123", scores);
        /// </code>
        /// </example>
        public async Task FinalizePartyScoresAsync(string roomCode, Dictionary<string, int> finalScores)
        {
            bool isInitialized = _isInitialized;
            bool hasValidRoomCode = !string.IsNullOrEmpty(roomCode);
            bool hasValidScores = finalScores != null;
            
            if (isInitialized && hasValidRoomCode && hasValidScores)
            {
                try
                {
                    await ProcessPlayerScoresAsync(roomCode, finalScores);
                    Debug.Log($"[FirebaseManager] Party {roomCode} scores finalized for {finalScores.Count} players");
                }
                catch (UserException ex)
                {
                    Debug.LogError($"[FirebaseManager] User operation failed: {ex.Message}");
                }
                catch (RepositoryException ex)
                {
                    Debug.LogError($"[FirebaseManager] Repository operation failed: {ex.Message}");
                }
                catch (Firebase.FirebaseException ex)
                {
                    Debug.LogError($"[FirebaseManager] Firebase error finalizing party scores: {ex.Message}");
                }
                catch (System.InvalidOperationException ex)
                {
                    Debug.LogError($"[FirebaseManager] Invalid operation finalizing party scores: {ex.Message}");
                }
            }
            else
            {
                if (!isInitialized)
                {
                    Debug.LogError("[FirebaseManager] Not initialized");
                }
                else if (!hasValidRoomCode)
                {
                    Debug.LogError("[FirebaseManager] Room code cannot be null or empty");
                }
                else if (!hasValidScores)
                {
                    Debug.LogError("[FirebaseManager] Final scores cannot be null");
                }
            }
        }
        
        /// <summary>
        /// Procesa las puntuaciones de cada jugador de forma asíncrona.
        /// Actualiza el historial de partidas y estadísticas individuales.
        /// </summary>
        /// <param name="roomCode">Código de la sala para historial</param>
        /// <param name="finalScores">Puntuaciones finales por jugador</param>
        /// <returns>Task que representa el procesamiento de todas las puntuaciones</returns>
        /// <remarks>
        /// Este método itera sobre cada entrada del diccionario y actualiza
        /// tanto el historial de partidas como las estadísticas del jugador.
        /// </remarks>
        private async Task ProcessPlayerScoresAsync(string roomCode, Dictionary<string, int> finalScores)
        {
            string userId;
            int score;
            
            foreach (var scoreEntry in finalScores)
            {
                userId = scoreEntry.Key;
                score = scoreEntry.Value;
                
                await UpdatePlayerGameHistory(userId, roomCode);
                await UpdatePlayerStatistics(userId, score);
                
                Debug.Log($"[FirebaseManager] Updated user {userId} with score {score}");
            }
        }
        
        /// <summary>
        /// Agrega una partida al historial de juegos de un usuario específico.
        /// </summary>
        /// <param name="userId">ID único del usuario</param>
        /// <param name="roomCode">Código de la sala jugada</param>
        /// <returns>Task que representa la operación de actualización</returns>
        private async Task UpdatePlayerGameHistory(string userId, string roomCode)
        {
            await _userRepository.AddGameToUserHistoryAsync(userId, roomCode);
        }
        
        /// <summary>
        /// Actualiza las estadísticas globales de un jugador después de una partida.
        /// Incluye puntos de experiencia (actualmente 0) y puntuación obtenida.
        /// </summary>
        /// <param name="userId">ID único del usuario</param>
        /// <param name="score">Puntuación obtenida en la partida</param>
        /// <returns>Task que representa la operación de actualización</returns>
        /// <remarks>
        /// EXPERIENCE_POINTS está configurado en 0 ya que el sistema de XP
        /// se maneja en otro componente del sistema.
        /// </remarks>
        private async Task UpdatePlayerStatistics(string userId, int score)
        {
            const int EXPERIENCE_POINTS = 0; 
            await _userRepository.UpdateUserStatsAsync(userId, EXPERIENCE_POINTS, score);
        }
        
        #region Public Repository Access
        
        /// <summary>Proporciona acceso al repositorio de partidos y salas</summary>
        /// <value>Instancia de PartyRepository para gestión de partidos</value>
        public PartyRepository PartyRepository => _partyRepository;
        
        /// <summary>Proporciona acceso al repositorio de usuarios y perfiles</summary>
        /// <value>Instancia de UserRepository para gestión de usuarios</value>
        public UserRepository UserRepository => _userRepository;
        
        /// <summary>Proporciona acceso al repositorio de sesiones activas</summary>
        /// <value>Instancia de SessionRepository para gestión de sesiones</value>
        public SessionRepository SessionRepository => _sessionRepository;
        
        #endregion
    }
}
