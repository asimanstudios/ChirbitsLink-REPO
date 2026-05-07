using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ChibitsLink.GameSide.HookParty
{
    public enum HookPartyState
    {
        Preparing,
        Countdown,
        InGame,
        Finished,
        TransitioningToLobby
    }

    /// <summary>
    /// Gestiona los tiempos y la máquina de estados del propio minijuego.
    /// </summary>
    public class HookPartyManager : MonoBehaviour
    {
        public static HookPartyManager Instance { get; private set; }

        [Header("Game Flow")]
        [Tooltip("Duración de la partida en segundos")]
        public float gameDurationSeconds = 60f;
        public float countdownSeconds = 3f;
        public float returnToLobbySeconds = 10f;
        public float playerScaleMultiplier = 1.5f;

        [Header("Debug / Animaciones")]
        public bool useAnimations = true;
        public RuntimeAnimatorController overrideAnimatorController;

        [Header("Audio (Opcional)")]
        public AudioClip countdownSound;
        public AudioClip startRoundSound;
        public AudioClip endRoundSound;
        [Range(0f, 2f)] public float globalVolume = 1.0f;

        [Header("Experiencia de Usuario (FX del Gancho inyectados)")]
        [Tooltip("Objeto pegado a los pies (ej. red de pescar/ataduras) mientras cuelgas")]
        public GameObject playerAttachmentPrefab;
        [Tooltip("Objeto que se pega en la pared (la punta metálica del gancho)")]
        public GameObject hookTipPrefab;
        public AudioClip shootSound;
        public AudioClip hitWallSound;
        public AudioClip cutRopeSound;

        [Header("Estado (Solo lectura)")]
        public HookPartyState CurrentState = HookPartyState.Preparing;
        public float TimeRemaining { get; private set; }
        public int CountdownValue { get; private set; }
        public float ReturnTimeRemaining { get; private set; }
        
        public bool IsPlaying => CurrentState == HookPartyState.InGame;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private IEnumerator Start()
        {
            // ESPERAR A PLAYERMANAGER: Debido al nuevo DelayedSpawnRoutine del PlayerManager,
            // debemos esperar un par de frames para asegurar que los jugadores existen físicamente.
            yield return new WaitForSeconds(0.2f);
            
            // AUTOMONTAJE DE SCRIPTS A LOS JUGADORES
            EscanearYConfigurarJugadores();
            
            // Inicializar el tiempo para que la UI no muestre 00:00 al cargar
            TimeRemaining = gameDurationSeconds;

            yield return StartCoroutine(SecuenciaInicio());
        }

        private void Update()
        {
            if (CurrentState == HookPartyState.InGame)
            {
                if (TimeRemaining > 0)
                {
                    TimeRemaining -= Time.deltaTime;
                    if (TimeRemaining <= 0)
                    {
                        TimeRemaining = 0;
                        StartCoroutine(SecuenciaFinal());
                    }
                }
            }
        }

        private IEnumerator SecuenciaInicio()
        {
            CurrentState = HookPartyState.Preparing;
            Debug.Log("[HookParty] Iniciando secuencia de cuenta atrás...");
            yield return new WaitForSeconds(0.5f); // Breve respiro tras la carga

            CurrentState = HookPartyState.Countdown;
            PlayGlobalSound(countdownSound);

            for (int i = (int)countdownSeconds; i > 0; i--)
            {
                CountdownValue = i;
                Debug.Log($"[HookParty] Cuenta atrás: {i}");
                yield return new WaitForSeconds(1f);
            }
            CountdownValue = 0;

            CurrentState = HookPartyState.InGame;
            TimeRemaining = gameDurationSeconds;
            PlayGlobalSound(startRoundSound);
            Debug.Log("[HookParty] ¡A COLUMPIARSE! Minijuego iniciado.");
        }

        private void EscanearYConfigurarJugadores()
        {
            GameObject[] found = GameObject.FindGameObjectsWithTag("Player");
            Debug.Log($"[HookParty] Detectados {found.Length} jugadores base. Equipándoles los ganchos de agarre...");

            foreach (var j in found)
            {
                // 1. Deshabilitar cualquier controlador original para que no interfiera 
                //    (ya que PlayerManager lee el primero que encuentra).
                var oldController = j.GetComponentInChildren<PlayerManager.IChibitsController>(true);
                if (oldController != null && oldController as MonoBehaviour != null)
                {
                    Destroy((oldController as MonoBehaviour));
                }

                // 2. Añadir los componentes de Hook Party necesarios
                if (j.GetComponent<HookPartyController>() == null)
                {
                    j.AddComponent<HookPartyController>();
                }
                
                var hookSys = j.GetComponent<PlayerHookSystem>();
                if (hookSys == null)
                {
                    hookSys = j.AddComponent<PlayerHookSystem>();
                }

                // Inyectamos la magia UX desde el Manager para no ensuciar los Prefabs genéricos de los personajes
                hookSys.SetupUX(playerAttachmentPrefab, hookTipPrefab, shootSound, hitWallSound, cutRopeSound, useAnimations, overrideAnimatorController);

                // Opcional: Si usan Visualizer, también
                if (j.GetComponent<PlayerAimVisualizer>() == null)
                {
                    j.AddComponent<PlayerAimVisualizer>();
                }
                
                // Asegurar física: Destruimos CharacterController si existe porque
                // sobreescribe la gravedad y anula al Rigidbody.
                var cc = j.GetComponent<CharacterController>();
                if (cc != null) Destroy(cc);

                var rb = j.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false; // El gancho requiere ser físico para usar fuerzas
                    rb.useGravity = true; 
                    rb.constraints = RigidbodyConstraints.FreezeRotation; // Para que no vuelque
                }

                // Ajustar Escala (Unicamente en este minijuego)
                j.transform.localScale = Vector3.one * playerScaleMultiplier;
            }
        }

        private IEnumerator SecuenciaFinal()
        {
            CurrentState = HookPartyState.Finished;
            PlayGlobalSound(endRoundSound);
            Debug.Log("[HookParty] ¡Tiempo Agotado! Todos al suelo.");

            // 1. Enviar puntuaciones de ítems recolectados al aire libre hacia Firestore.
            if (ScoreManager.Instance != null && ChibitsLink.GameSide.LobbyManager.Instance != null && ChibitsLink.GameSide.TcpServer.Instance != null)
            {
                var scores = ScoreManager.Instance.GetAllScores();
                string code = ChibitsLink.GameSide.TcpServer.Instance.GetRoomCode();

                if (!string.IsNullOrEmpty(code))
                {
                    string userId;
                    int puntosItem;
                    
                    foreach (var scoreEntry in scores)
                    {
                        userId = scoreEntry.Key;
                        puntosItem = scoreEntry.Value; 

                        Debug.Log($"[HookParty] Subiendo {puntosItem} ptos (XP) al jugador {userId}");
                        _ = ChibitsLink.GameSide.LobbyManager.Instance.UpdatePlayerScoreAsync(code, userId, puntosItem);
                    }

                    // Confirmar para pasar los puntos a experiencia oficial en el server
                    _ = ChibitsLink.GameSide.LobbyManager.Instance.FinalizePartyScoresAsync(code);
                }
            }

            // 2. Transición pacífica para ver puntajes locales
            CurrentState = HookPartyState.TransitioningToLobby;
            ReturnTimeRemaining = returnToLobbySeconds;

            while (ReturnTimeRemaining > 0)
            {
                yield return new WaitForSeconds(1f);
                ReturnTimeRemaining--;
            }

            // 3. Retorno al Lobby global
            string roomCode = ChibitsLink.GameSide.TcpServer.Instance?.GetRoomCode() ?? "";
            if (!string.IsNullOrEmpty(roomCode) && ChibitsLink.GameSide.LobbyManager.Instance != null)
            {
                _ = ChibitsLink.GameSide.LobbyManager.Instance.ReturnToLobbyAsync(roomCode);
            }
            else
            {
                Debug.LogWarning("[HookParty] LobbyManager no disponible. Retornando forzosamente a Menu.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("menu");
            }
        }

        public void PlayGlobalSound(AudioClip clip)
        {
            if (clip != null)
            {
                Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(clip, pos, globalVolume);
            }
        }
    }
}
