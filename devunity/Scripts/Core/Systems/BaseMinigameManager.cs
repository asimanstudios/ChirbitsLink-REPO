using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chirbits.Core.Exceptions;

namespace Chirbits.Core
{
    /// <summary>
    /// Estados posibles de un minijuego.
    /// Define el ciclo de vida del juego.
    /// </summary>
    public enum MinigameState
    {
        /// <summary>Fase de preparación inicial</summary>
        Preparing,
        /// <summary>Fase de cuenta regresiva</summary>
        Countdown,
        /// <summary>Fase de juego activo</summary>
        InGame,
        /// <summary>Fase de resultados</summary>
        Result,
        /// <summary>Fase de finalización</summary>
        Ending
    }

    /// <summary>
    /// Clase base para todos los minijuegos de Chirbits.
    /// Gestiona el ciclo de vida (FSM) y la comunicación con el Lobby.
    /// Implementa IMinigameManager para consistencia.
    /// </summary>
    /// <remarks>
    /// Proporciona estructura común para todos los minijuegos.
    /// Maneja secuencia automática: preparación, cuenta regresiva, juego, resultados.
    /// Facilita comunicación con LobbyManager y reporte de puntuaciones.
    /// </remarks>
    public abstract class BaseMinigameManager : MonoBehaviour, IMinigameManager
    {
        [Header("Configuración Base")]
        /// <summary>Tiempo de cuenta regresiva (segundos)</summary>
        public float countdownTime = 3f;
        /// <summary>Tiempo de pantalla de resultados (segundos)</summary>
        public float resultTime = 5f;
        /// <summary>Nombre del minijuego para logs</summary>
        public string minigameName = "Minijuego";

        [Header("Audio")]
        /// <summary>Sonido de cuenta regresiva</summary>
        public AudioClip countdownSound;
        /// <summary>Sonido de inicio de juego</summary>
        public AudioClip startSound;
        /// <summary>Sonido de victoria/finalización</summary>
        public AudioClip victorySound;

        /// <summary>Estado actual del minijuego</summary>
        protected MinigameState currentState = MinigameState.Preparing;
        /// <summary>Lista de jugadores detectados</summary>
        protected List<GameObject> players = new List<GameObject>();
        /// <summary>Cache de identidades de jugadores</summary>
        protected Dictionary<GameObject, ChibitsLink.GameSide.PlayerIdentity> identityCache = new Dictionary<GameObject, ChibitsLink.GameSide.PlayerIdentity>();
        /// <summary>Componente de audio</summary>
        protected AudioSource audioSource;

        /// <summary>
        /// Indica si el juego está actualmente en ejecución.
        /// Implementación de IMinigameManager.
        /// </summary>
        public bool IsGameRunning => currentState == MinigameState.InGame;

        /// <summary>
        /// Obtiene la identidad de un jugador.
        /// Utiliza cache para optimizar rendimiento.
        /// </summary>
        /// <param name="obj">GameObject del jugador</param>
        /// <returns>Identidad del jugador o null</returns>
        public ChibitsLink.GameSide.PlayerIdentity GetIdentity(GameObject obj)
        {
            ChibitsLink.GameSide.PlayerIdentity resolvedIdentity = null;
            if (obj != null)
            {
                if (identityCache.TryGetValue(obj, out var id))
                {
                    resolvedIdentity = id;
                }
                else
                {
                    // Fallback for objects added late or not in initial scan
                    var manual = obj.GetComponent<ChibitsLink.GameSide.PlayerIdentity>() ?? obj.GetComponentInParent<ChibitsLink.GameSide.PlayerIdentity>();
                    if (manual != null)
                    {
                        identityCache[obj] = manual;
                    }

                    resolvedIdentity = manual;
                }
            }

            return resolvedIdentity;
        }

        /// <summary>
        /// Inicialización del gestor del minijuego.
        /// Configura componente de audio.
        /// </summary>
        protected virtual void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Inicio del minijuego.
        /// Inicia la secuencia del juego.
        /// </summary>
        protected virtual void Start()
        {
            StartCoroutine(GameSequenceCoroutine());
        }

        /// <summary>
        /// Corrutina principal de secuencia del juego.
        /// Orquesta todas las fases del minijuego.
        /// </summary>
        /// <returns>IEnumerator para la corrutina</returns>
        private IEnumerator GameSequenceCoroutine()
        {
            // 1. PREPARACIÓN
            currentState = MinigameState.Preparing;
            OnGamePreparing();
            yield return new WaitForSeconds(1.5f);
            ScanPlayers();

            // 2. CUENTA ATRÁS
            currentState = MinigameState.Countdown;
            if (countdownSound) audioSource.PlayOneShot(countdownSound);
            for (int i = (int)countdownTime; i > 0; i--)
            {
                OnCountdownTick(i);
                yield return new WaitForSeconds(1f);
            }

            // 3. EN JUEGO
            currentState = MinigameState.InGame;
            if (startSound) audioSource.PlayOneShot(startSound);
            OnGameStarted();
            
            // Esperar a que una subclase o condición termine el juego
            yield return WaitUntilGameEnds();

            // 4. RESULTADOS
            currentState = MinigameState.Result;
            if (victorySound) audioSource.PlayOneShot(victorySound);
            OnGameResults();
            yield return new WaitForSeconds(resultTime);

            // 5. FINALIZAR (Volver al lobby)
            currentState = MinigameState.Ending;
            EndGame();
        }

        /// <summary>
        /// Escanea y cachea jugadores en la escena.
        /// Busca objetos con tag "Player" y sus identidades.
        /// </summary>
        protected virtual void ScanPlayers()
        {
            var tagged = GameObject.FindGameObjectsWithTag("Player");
            players.Clear();
            identityCache.Clear();
            foreach (var g in tagged)
            {
                players.Add(g);
                var id = g.GetComponent<ChibitsLink.GameSide.PlayerIdentity>() ?? g.GetComponentInParent<ChibitsLink.GameSide.PlayerIdentity>();
                if (id != null) identityCache[g] = id;
            }
            Debug.Log($"[{minigameName}] Detectados {players.Count} jugadores y cache de identidades preparada.");
        }

        // --- Métodos abstractos/virtuales para las subclases ---
        
        /// <summary>
        /// Evento cuando el juego está en preparación.
        /// Las subclases pueden sobreescribir para configuración específica.
        /// </summary>
        protected virtual void OnGamePreparing() { }
        
        /// <summary>
        /// Evento en cada tick de la cuenta regresiva.
        /// </summary>
        /// <param name="tick">Número actual de la cuenta regresiva</param>
        protected virtual void OnCountdownTick(int tick) { }
        
        /// <summary>
        /// Evento cuando el juego comienza.
        /// Las subclases deben implementar la lógica de inicio.
        /// </summary>
        protected abstract void OnGameStarted();
        
        /// <summary>
        /// Espera hasta que el juego termine.
        /// Las subclases deben implementar la condición de fin.
        /// </summary>
        /// <returns>IEnumerator para la espera</returns>
        protected abstract IEnumerator WaitUntilGameEnds();
        
        /// <summary>
        /// Evento cuando se muestran los resultados.
        /// Las subclases pueden sobreescribir para mostrar resultados específicos.
        /// </summary>
        protected virtual void OnGameResults() { }

        /// <summary>
        /// Inicia el juego.
        /// Implementación de IMinigameManager.
        /// </summary>
        public virtual void StartGame()
        {
            // Ya iniciado por la corrutina en Start()
        }

        /// <summary>
        /// Finaliza el juego y regresa al lobby.
        /// Implementación de IMinigameManager.
        /// </summary>
        public virtual void EndGame()
        {
            Debug.Log($"[{minigameName}] Finalizando partida y notificando a LobbyManager...");
            
            var lobby = GameObject.FindObjectOfType<ChibitsLink.GameSide.LobbyManager>();
            if (lobby == null)
            {
                throw new SessionLogicException("No se encontró el LobbyManager para volver.");
            }

            lobby.ReturnToLobby();
        }

        /// <summary>
        /// Reproduce un sonido de efecto.
        /// </summary>
        /// <param name="clip">Clip de audio a reproducir</param>
        public void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// Reporta puntuación de un jugador al lobby.
        /// Actualiza tanto puntos de sala como experiencia global.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="points">Puntos obtenidos</param>
        protected void ReportScore(string userId, int points)
        {
            var lobby = GameObject.FindObjectOfType<ChibitsLink.GameSide.LobbyManager>();
            if (lobby != null)
            {
                // 1. Reportar a la SALA (Puntos de la sesión/partida)
                _ = lobby.UpdatePlayerScoreAsync(lobby.RoomCode, userId, points).ContinueWith(t => {
                    if (t.IsFaulted) Debug.LogError($"[BaseManager] Error reportando puntos de sala: {t.Exception}");
                });

                // 2. Reportar al USUARIO (XP global para niveles)
                // DEPRECATED: El XP ahora lo reclama la App MAUI al cerrar la sala para evitar colisiones de datos.
                /*
                _ = lobby.AddUserExperienceAsync(userId, points).ContinueWith(t => {
                    if (t.IsFaulted) Debug.LogError($"[BaseManager] Error reportando XP global: {t.Exception}");
                });
                */
            }
        }
    }
}
