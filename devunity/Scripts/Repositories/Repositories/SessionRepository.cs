using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Extensions;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.Repositories
{
    /// <summary>
    /// Repositorio para operaciones de base de datos de sesiones de juego.
    /// Maneja todas las operaciones CRUD para sesiones sin lógica de negocio.
    /// </summary>
    /// <remarks>
    /// Se centra únicamente en operaciones de base de datos.
    /// Proporciona validación de datos y manejo de errores.
    /// Utiliza Firebase Firestore para persistencia.
    /// </remarks>
    public class SessionRepository
    {
        /// <summary>Instancia de Firebase Firestore</summary>
        private readonly FirebaseFirestore _database;
        /// <summary>Nombre de la colección</summary>
        private readonly string _collectionName = "game_sessions";
        /// <summary>Indica si está inicializado</summary>
        private bool _isInitialized;

        /// <summary>
        /// Constructor del repositorio.
        /// Inicializa la instancia de Firebase Firestore.
        /// </summary>
        /// <param name="database">Instancia de Firebase Firestore</param>
        /// <exception cref="ArgumentNullException">Si database es null</exception>
        public SessionRepository(FirebaseFirestore database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _isInitialized = true;
        }

        /// <summary>
        /// Actualiza una sesión de juego de forma asíncrona.
        /// Actualiza jugadores activos y puntos totales.
        /// </summary>
        /// <param name="hostId">ID del host de la sesión</param>
        /// <param name="activePlayers">Número de jugadores activos</param>
        /// <param name="totalPoints">Puntos totales de la sesión</param>
        /// <exception cref="SessionUpdateException">Si falla la actualización</exception>
        public async Task UpdateGameSessionAsync(string hostId, int activePlayers, int totalPoints)
        {
            ValidateRepositoryInitialized();
            ValidateHostId(hostId);
            ValidateSessionData(activePlayers, totalPoints);

            try
            {
                var docRef = _database.Collection(_collectionName).Document(hostId);
                var sessionData = new Dictionary<string, object>
                {
                    { "host_id", hostId },
                    { "active_players", activePlayers },
                    { "total_points", totalPoints },
                    { "last_updated", Timestamp.GetCurrentTimestamp() }
                };

                await docRef.SetAsync(sessionData, SetOptions.MergeAll);
            }
            catch (FirebaseException ex)
            {
                throw new SessionUpdateException($"Failed to update game session for host {hostId}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error updating game session for host {hostId}", ex);
            }
        }
        
        /// <summary>
        /// Valida los datos de sesión.
        /// Verifica que los valores sean válidos y no negativos.
        /// </summary>
        /// <param name="activePlayers">Número de jugadores activos</param>
        /// <param name="totalPoints">Puntos totales de la sesión</param>
        /// <exception cref="ArgumentException">Si los valores son negativos</exception>
        private void ValidateSessionData(int activePlayers, int totalPoints)
        {
            if (activePlayers < 0)
            {
                throw new ArgumentException("Active players cannot be negative", nameof(activePlayers));
            }

            if (totalPoints < 0)
            {
                throw new ArgumentException("Total points cannot be negative", nameof(totalPoints));
            }
        }
        
        /// <summary>
        /// Valida que el repositorio esté inicializado.
        /// Verifica que se haya llamado al constructor correctamente.
        /// </summary>
        /// <exception cref="RepositoryNotInitializedException">Si no está inicializado</exception>
        private void ValidateRepositoryInitialized()
        {
            if (!_isInitialized)
            {
                throw new RepositoryNotInitializedException("SessionRepository not initialized");
            }
        }
        
        /// <summary>
        /// Valida el ID del host.
        /// Verifica que no sea nulo o vacío.
        /// </summary>
        /// <param name="hostId">ID del host a validar</param>
        /// <exception cref="ArgumentNullException">Si hostId es null o vacío</exception>
        private void ValidateHostId(string hostId)
        {
            if (string.IsNullOrEmpty(hostId))
            {
                throw new ArgumentNullException(nameof(hostId));
            }
        }
        /// <summary>
        /// Obtiene una sesión de juego de forma asíncrona.
        /// Busca por ID de host y retorna los datos de la sesión.
        /// </summary>
        /// <param name="hostId">ID del host de la sesión</param>
        /// <returns>Diccionario con datos de la sesión o null</returns>
        /// <exception cref="SessionRetrievalException">Si falla la recuperación</exception>
        public async Task<Dictionary<string, object>> GetGameSessionAsync(string hostId)
        {
            ValidateRepositoryInitialized();
            ValidateHostId(hostId);

            Dictionary<string, object> result = null;
            
            try
            {
                var docRef = _database.Collection(_collectionName).Document(hostId);
                var snapshot = await docRef.GetSnapshotAsync();
                
                if (snapshot.Exists)
                {
                    result = snapshot.ToDictionary();
                }
                else
                {
                    throw new SessionNotFoundException($"Game session for host {hostId} not found");
                }
            }
            catch (FirebaseException ex)
            {
                throw new SessionRetrievalException($"Failed to retrieve game session for host {hostId}", ex);
            }
            catch (SessionNotFoundException)
            {
                // SessionNotFoundException already thrown above, let it bubble up
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error retrieving game session for host {hostId}", ex);
            }
            
            return result;
        }

        public async Task DeleteGameSessionAsync(string hostId)
        {
            ValidateRepositoryInitialized();
            ValidateHostId(hostId);

            try
            {
                var docRef = _database.Collection(_collectionName).Document(hostId);
                await docRef.DeleteAsync();
            }
            catch (FirebaseException ex)
            {
                throw new SessionDeletionException($"Failed to delete game session for host {hostId}", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unexpected error deleting game session for host {hostId}", ex);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetActiveSessionsAsync()
        {
            ValidateRepositoryInitialized();

            var result = new List<Dictionary<string, object>>();
            
            try
            {
                var query = _database.Collection(_collectionName)
                    .OrderByDescending("last_updated")
                    .Limit(50); // Limit to recent sessions

                var snapshot = await query.GetSnapshotAsync();

                foreach (var document in snapshot.Documents)
                {
                    result.Add(document.ToDictionary());
                }
            }
            catch (FirebaseException ex)
            {
                throw new SessionRetrievalException("Failed to retrieve active sessions", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Unexpected error retrieving active sessions", ex);
            }
            
            return result;
        }
    }
}