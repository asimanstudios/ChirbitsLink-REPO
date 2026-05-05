using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chirbits.Core.Exceptions;

namespace Chirbits.Core
{
    public enum MinigameState
    {
        Preparing,
        Countdown,
        InGame,
        Result,
        Ending
    }

    /// <summary>
    /// Clase base para todos los minijuegos de Chirbits.
    /// Gestiona el ciclo de vida (FSM) y la comunicación con el Lobby.
    /// </summary>
    public abstract class BaseMinigameManager : MonoBehaviour, IMinigameManager
    {
        [Header("Configuración Base")]
        public float countdownTime = 3f;
        public float resultTime = 5f;
        public string minigameName = "Minijuego";

        [Header("Audio")]
        public AudioClip countdownSound;
        public AudioClip startSound;
        public AudioClip victorySound;

        protected MinigameState currentState = MinigameState.Preparing;
        protected List<GameObject> players = new List<GameObject>();
        protected Dictionary<GameObject, ChibitsLink.GameSide.PlayerIdentity> identityCache = new Dictionary<GameObject, ChibitsLink.GameSide.PlayerIdentity>();
        protected AudioSource audioSource;

        public bool IsGameRunning => currentState == MinigameState.InGame;

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

        protected virtual void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            StartCoroutine(GameSequenceCoroutine());
        }

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
        
        protected virtual void OnGamePreparing() { }
        protected virtual void OnCountdownTick(int tick) { }
        protected abstract void OnGameStarted();
        protected abstract IEnumerator WaitUntilGameEnds();
        protected virtual void OnGameResults() { }

        public virtual void StartGame()
        {
            // Ya iniciado por la corrutina en Start()
        }

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

        public void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

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
