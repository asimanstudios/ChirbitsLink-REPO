using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chirbits.Core;
using ChibitsLink.GameSide;

namespace ChibitsLink.Minigames.BombTag
{
    /// <summary>
    /// Enumeración que representa los estados específicos del minijuego BombTag.
    /// Define las fases del ciclo de vida del juego de etiquetas con bomba.
    /// </summary>
    public enum BombTagState { Preparing, Countdown, InGame, Result, Ending }

    /// <summary>
    /// Gestor específico del minijuego BombTag.
    /// Controla la lógica del juego de etiquetas con bombas y puntuación.
    /// Hereda de BaseMinigameManager para integración con el sistema general.
    /// </summary>
    /// <remarks>
    /// Implementa patrón Singleton para acceso global durante el minijuego.
    /// Orquesta física, puntuación y UI del juego BombTag.
    /// </remarks>
    public class BombTagGameManager : BaseMinigameManager
    {
        /// <summary>Instancia global del gestor BombTag (patrón Singleton)</summary>
        public static BombTagGameManager Instance { get; private set; }
        
        [Header("Runtime State")]
        /// <summary>Tiempo restante antes de que explote la bomba</summary>
        public float remainingTime;
        /// <summary>Jugador que actualmente porta la bomba</summary>
        public GameObject carrier;
        
        // Components
        /// <summary>Sistema de física para manejo de bombas y transferencias</summary>
        private BombTagPhysics _physics;
        /// <summary>Sistema de puntuación y estadísticas del juego</summary>
        private BombTagScoring _scoring;
        /// <summary>Interfaz de usuario específica del minijuego</summary>
        private BombTagUI _ui;
        
        // Game State
        /// <summary>Configuración del juego BombTag</summary>
        private BombaTag _config;
        /// <summary>Valor actual de la cuenta regresiva</summary>
        private int _countdownValue;
        /// <summary>Lista de jugadores participantes</summary>
        private List<GameObject> _players = new List<GameObject>();
        /// <summary>Diccionario de identidades de jugadores</summary>
        private Dictionary<GameObject, PlayerIdentity> _playerIdentities = new Dictionary<GameObject, PlayerIdentity>();

        /// <summary>
        /// Inicializa el gestor del minijuego y establece el patrón Singleton.
        /// Configura los componentes necesarios para el funcionamiento.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            InitializeComponents();
        }
        
        /// <summary>
        /// Inicializa los componentes requeridos por el gestor.
        /// Crea componentes si no existen y los configura apropiadamente.
        /// </summary>
        /// <remarks>
        /// Agrega componentes dinámicamente si no se encuentran.
        /// Inicializa la UI con las dependencias necesarias.
        /// </remarks>
        private void InitializeComponents()
        {
            _physics = GetComponent<BombTagPhysics>();
            if (_physics == null)
            {
                _physics = gameObject.AddComponent<BombTagPhysics>();
            }
            
            _scoring = GetComponent<BombTagScoring>();
            if (_scoring == null)
            {
                _scoring = gameObject.AddComponent<BombTagScoring>();
            }
            
            _ui = GetComponent<BombTagUI>();
            if (_ui == null)
            {
                _ui = gameObject.AddComponent<BombTagUI>();
            }
            
            // Initialize components
            _ui.Initialize(_scoring, this);
        }

        /// <summary>
        /// Maneja la fase de preparación del minijuego.
        /// Cachea identidades de jugadores y configura estado inicial.
        /// </summary>
        /// <remarks>
        /// Se llama automáticamente por el sistema de minijuegos.
        /// Prepara todos los sistemas para iniciar la partida.
        /// </remarks>
        protected override void OnGamePreparing()
        {
            Debug.Log("[BombTagGameManager] OnGamePreparing - Caching identities");
            
            ResetGameState();
            CachePlayerIdentities();
            FindConfiguration();
            
            Debug.Log($"[BombTagGameManager] {_players.Count} players ready.");
        }
        
        /// <summary>
        /// Resetea el estado del juego a valores iniciales.
        /// Limpia variables y reinicia sistemas hijos.
        /// </summary>
        private void ResetGameState()
        {
            remainingTime = 0f;
            carrier = null;
            _countdownValue = 0;
            
            _scoring.Initialize(_playerIdentities);
            _physics.DestroyBomb();
        }
        
        /// <summary>
        /// Cachea las identidades de todos los jugadores válidos.
        /// Filtra y almacena jugadores con componentes válidos.
        /// </summary>
        /// <remarks>
        /// Evita llamadas a GetComponent durante el gameplay.
        /// Almacena tanto GameObjects como PlayerIdentities.
        /// </remarks>
        private void CachePlayerIdentities()
        {
            ClearPlayerCaches();
            var validPlayers = GetValidPlayers();
            
            foreach (GameObject player in validPlayers)
            {
                CachePlayerIdentity(player);
            }
        }
        
        /// <summary>
        /// Limpia los caches de jugadores.
        /// Prepara las colecciones para nueva cacheada.
        /// </summary>
        private void ClearPlayerCaches()
        {
            _players.Clear();
            _playerIdentities.Clear();
        }
        
        /// <summary>
        /// Obtiene la lista de jugadores válidos actualmente.
        /// Filtra jugadores nulos o destruidos.
        /// </summary>
        /// <returns>Lista de jugadores válidos</returns>
        /// <remarks>
        /// Filtra inmediatamente para evitar problemas durante gameplay.
        /// </remarks>
        private List<GameObject> GetValidPlayers()
        {
            // Filter and cache immediately to avoid GetComponent during gameplay
            return players.Where(p => p != null).ToList();
        }
        
        /// <summary>
        /// Cachea la identidad de un jugador específico.
        /// Obtiene el componente PlayerIdentity del jugador.
        /// </summary>
        /// <param name="player">GameObject del jugador a cachear</param>
        private void CachePlayerIdentity(GameObject player)
        {
            PlayerIdentity identity = GetPlayerIdentity(player);
            if (identity != null)
            {
                _players.Add(player);
                _playerIdentities[player] = identity;
            }
        }
        
        /// <summary>
        /// Obtiene la identidad de un jugador.
        /// Busca en el objeto y sus padres el componente PlayerIdentity.
        /// </summary>
        /// <param name="player">GameObject del jugador</param>
        /// <returns>PlayerIdentity encontrada o null</returns>
        private PlayerIdentity GetPlayerIdentity(GameObject player)
        {
            return player.GetComponent<PlayerIdentity>() ?? player.GetComponentInParent<PlayerIdentity>();
        }
        
        /// <summary>
        /// Busca la configuración del juego BombTag.
        /// Encuentra el componente BombaTag con prefabs válidos.
        /// </summary>
        /// <remarks>
        /// Busca en toda la escena componentes BombaTag.
        /// Selecciona el primero que tenga bombPrefab configurado.
        /// </remarks>
        private void FindConfiguration()
        {
            _config = FindObjectsByType<BombaTag>(FindObjectsSortMode.None)
                .FirstOrDefault(c => c.bombPrefab != null);
            
            if (_config != null)
            {
                _physics.Initialize(_config);
            }
            else
            {
                Debug.LogWarning("[BombTagGameManager] No BombaTag configuration found");
            }
        }

        /// <summary>
        /// Maneja cada tick de la cuenta regresiva.
        /// Actualiza el valor mostrado en la UI.
        /// </summary>
        /// <param name="tick">Valor actual de la cuenta regresiva</param>
        protected override void OnCountdownTick(int tick) 
        { 
            _countdownValue = tick; 
        }

        /// <summary>
        /// Maneja el inicio del minijuego.
        /// Inicia una nueva ronda con un jugador aleatorio.
        /// </summary>
        protected override void OnGameStarted()
        {
            StartNewRound();
        }

        /// <summary>
        /// Inicia una nueva ronda del juego.
        /// Selecciona una víctima aleatoria y spawnea la bomba.
        /// </summary>
        /// <remarks>
        /// Solo inicia si hay más de un jugador vivo.
        /// Selecciona aleatoriamente quién recibirá la bomba primero.
        /// </remarks>
        private void StartNewRound()
        {
            bool canStartRound = IsGameRunning;
            if (canStartRound)
            {
                List<GameObject> alivePlayers = GetAlivePlayers();
                canStartRound = alivePlayers.Count > 1;
                
                if (canStartRound)
                {
                    GameObject victim = SelectRandomVictim(alivePlayers);
                    Debug.Log($"[BombTagGameManager] Starting round. Target: {victim.name}");
                    
                    _physics.SpawnBomb(victim);
                    remainingTime = _config.bombDuration;
                    carrier = victim;
                }
            }
        }
        
        /// <summary>
        /// Selecciona aleatoriamente una víctima de la lista de jugadores.
        /// </summary>
        /// <param name="alivePlayers">Lista de jugadores vivos</param>
        /// <returns>Jugador seleccionado aleatoriamente</returns>
        /// <remarks>
        /// Retorna null si la lista está vacía.
        /// </remarks>
        private GameObject SelectRandomVictim(List<GameObject> alivePlayers)
        {
            GameObject victim = null;
            if (alivePlayers.Count > 0)
            {
                victim = alivePlayers[Random.Range(0, alivePlayers.Count)];
            }
            return victim;
        }


        /// <summary>
        /// Corutina principal que espera hasta que termine el juego.
        /// Procesa el bucle de juego y finaliza cuando corresponde.
        /// </summary>
        /// <returns>IEnumerator para la corutina</returns>
        protected override IEnumerator WaitUntilGameEnds()
        {
            bool keepRunning = IsGameRunning;
            while (keepRunning)
            {
                yield return null;
                keepRunning = ProcessGameLoop();
            }

            yield return FinalizeGameSequence();
        }
        
        /// <summary>
        /// Procesa la lógica principal del bucle de juego.
        /// Actualiza transferencias, temporizador y condiciones de victoria.
        /// </summary>
        /// <returns>True si el juego debe continuar</returns>
        /// <remarks>
        /// Detiene la lógica durante explosiones.
        /// Finaliza cuando queda un solo jugador.
        /// </remarks>
        private bool ProcessGameLoop()
        {
            // Stop logic while exploding
            if (_physics.IsExploding())
            {
                return IsGameRunning;
            }
            
            List<GameObject> alivePlayers = GetAlivePlayers();
            bool hasSingleSurvivor = alivePlayers.Count <= 1;
            
            if (hasSingleSurvivor)
            {
                return false;
            }
            
            UpdateGameLogic(alivePlayers);
            
            if (remainingTime <= 0)
            {
                remainingTime = 0;
                StartCoroutine(_physics.ProcessExplosion());
            }
            
            return true;
        }
        
        /// <summary>
        /// Corutina que finaliza la secuencia del juego.
        /// Espera antes de mostrar resultados finales.
        /// </summary>
        /// <returns>IEnumerator para la corutina</returns>
        private IEnumerator FinalizeGameSequence()
        {
            Debug.Log("[BombTagGameManager] Game loop finished. Waiting before results...");
            yield return new WaitForSecondsRealtime(0.5f);
            FinalizeGame();
        }
        
        /// <summary>
        /// Actualiza la lógica del juego cada frame.
        /// Procesa transferencias y actualiza temporizador y visuales.
        /// </summary>
        /// <param name="alivePlayers">Lista de jugadores actualmente vivos</param>
        private void UpdateGameLogic(List<GameObject> alivePlayers)
        {
            // Update physics and transfers
            bool transferOccurred = _physics.UpdateTransfer(alivePlayers);
            if (transferOccurred)
            {
                carrier = _physics.GetCurrentCarrier();
            }
            
            // Update bomb visuals
            _physics.UpdateBombVisuals(remainingTime);
            
            // Update timer
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
            }
        }


        /// <summary>
        /// Finaliza el juego y procesa puntuaciones.
        /// Llama al sistema de puntuación para calcular resultados.
        /// </summary>
        private void FinalizeGame()
        {
            _scoring.ProcessFinalScoring(ReportScore);
        }

        /// <summary>
        /// Obtiene la lista de jugadores actualmente vivos.
        /// Filtra jugadores activos y no destruidos.
        /// </summary>
        /// <returns>Lista de jugadores vivos</returns>
        /// <remarks>
        /// Verifica activeInHierarchy para jugadores válidos.
        /// </remarks>
        private List<GameObject> GetAlivePlayers()
        {
            List<GameObject> alive = new List<GameObject>();
            GameObject player;
            
            for (int i = 0; i < _players.Count; i++)
            {
                player = _players[i];
                if (player != null && player.activeInHierarchy)
                {
                    alive.Add(player);
                }
            }
            
            return alive;
        }

        // --- UI API ---
        
        /// <summary>
        /// Obtiene el nombre del jugador que actualmente porta la bomba.
        /// </summary>
        /// <returns>Nombre del portador o "None" si no hay bomba</returns>
        public string GetCarrierName()
        {
            string name = "None";
            if (carrier != null)
            {
                name = _scoring.GetPlayerName(carrier);
            }
            return name;
        }
        
        /// <summary>
        /// Obtiene la cantidad de jugadores actualmente vivos.
        /// </summary>
        /// <returns>Número de jugadores vivos</returns>
        public int GetAliveCount() => GetAlivePlayers().Count;
        /// <summary>
        /// Obtiene la lista de jugadores ganadores (jugadores vivos).
        /// </summary>
        /// <returns>Lista de jugadores que sobrevivieron</returns>
        public List<GameObject> GetWinners() => GetAlivePlayers();
        /// <summary>
        /// Obtiene el orden en que los jugadores fueron eliminados.
        /// </summary>
        /// <returns>Lista ordenada de jugadores eliminados</returns>
        public List<GameObject> GetEliminationOrder() => _scoring.GetEliminationOrder();
        
        /// <summary>
        /// Obtiene el nombre de un jugador específico.
        /// </summary>
        /// <param name="player">GameObject del jugador</param>
        /// <returns>Nombre del jugador o "Unknown" si es null</returns>
        public string GetPlayerName(GameObject player)
        {
            string name = "Unknown";
            if (player != null)
            {
                name = _scoring.GetPlayerName(player);
            }
            return name;
        }

        /// <summary>
        /// Obtiene el estado actual del juego en formato BombTagState.
        /// Convierte el estado base del minijuego al estado específico.
        /// </summary>
        /// <returns>Estado actual del juego BombTag</returns>
        public BombTagState CurrentState => currentState switch {
            MinigameState.Preparing => BombTagState.Preparing,
            MinigameState.Countdown => BombTagState.Countdown,
            MinigameState.InGame    => BombTagState.InGame,
            MinigameState.Result    => BombTagState.Result,
            MinigameState.Ending    => BombTagState.Ending,
            _                       => BombTagState.Preparing
        };
        
        /// <summary>Tiempo restante de la bomba actual</summary>
        public float remainingBombTime => remainingTime;
        /// <summary>Valor actual de la cuenta regresiva</summary>
        public int currentCountdown => _countdownValue;
        /// <summary>Tiempo de resultados (heredado de base)</summary>
        public float resultTime => resultTime;
    }
}
