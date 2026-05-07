using UnityEngine;
using Unity.Netcode;
using ChibiCocina.Models;

namespace ChibitsLink.Core
{
    /// <summary>
    /// Gestor principal del ciclo de vida del juego.
    /// Controla estados de juego, temporización y gestión de sesiones de jugadores.
    /// Implementa patrón Singleton para acceso global al estado del juego.
    /// </summary>
    /// <remarks>
    /// Orquesta todos los componentes principales del juego.
    /// Gestiona transiciones entre estados y eventos del sistema.
    /// </remarks>
    public class GameManager : MonoBehaviour
    {
        /// <summary>Instancia global del gestor del juego (patrón Singleton)</summary>
        public static GameManager Instance { get; private set; }
        
        [Header("Game Configuration")]
        /// <summary>Número máximo de jugadores permitidos en la partida</summary>
        public int maxPlayers = 4;
        /// <summary>Tiempo de preparación antes de iniciar el juego (segundos)</summary>
        public float preparationTime = 5f;
        /// <summary>Duración total de la partida (segundos)</summary>
        public float gameTime = 300f;
        
        // Components
        /// <summary>Gestor de temporización del juego</summary>
        private GameTimer _gameTimer;
        /// <summary>Gestor de sesiones de jugadores conectados</summary>
        private PlayerSessionManager _playerSessionManager;
        /// <summary>Interfaz de depuración para información del juego</summary>
        private GameDebugUI _debugUI;
        
        // Game State
        /// <summary>Estado actual del ciclo de vida del juego</summary>
        private GameState _currentState;
        
        // Events
        /// <summary>Evento disparado cuando cambia el estado del juego</summary>
        public System.Action<GameState> OnGameStateChanged;
        /// <summary>Evento disparado cuando se actualiza el tiempo restante</summary>
        public System.Action<float> OnTimeUpdated;
        /// <summary>Evento disparado cuando cambia la cantidad de jugadores</summary>
        public System.Action<int> OnPlayersUpdated;
        
        /// <summary>
        /// Inicializa el gestor del juego y establece el patrón Singleton.
        /// Configura componentes y estado inicial del juego.
        /// </summary>
        /// <remarks>
        /// Utiliza DontDestroyOnLoad para persistir entre escenas.
        /// Destruye instancias duplicadas automáticamente.
        /// </remarks>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Inicializa todos los sistemas del juego.
        /// Configura componentes y establece el estado inicial.
        /// </summary>
        private void InitializeGame()
        {
            InitializeComponents();
            SetInitialState();
            
            Debug.Log("[GameManager] Initialized in waiting state");
        }
        
        /// <summary>
        /// Inicializa los componentes requeridos por el gestor.
        /// Crea componentes si no existen y los configura apropiadamente.
        /// </summary>
        /// <remarks>
        /// Agrega componentes dinámicamente si no se encuentran en el GameObject.
        /// Suscribe a eventos de los componentes hijos.
        /// </remarks>
        private void InitializeComponents()
        {
            _gameTimer = GetComponent<GameTimer>();
            if (_gameTimer == null)
            {
                _gameTimer = gameObject.AddComponent<GameTimer>();
            }
            
            _playerSessionManager = GetComponent<PlayerSessionManager>();
            if (_playerSessionManager == null)
            {
                _playerSessionManager = gameObject.AddComponent<PlayerSessionManager>();
            }
            
            _debugUI = GetComponent<GameDebugUI>();
            if (_debugUI == null)
            {
                _debugUI = gameObject.AddComponent<GameDebugUI>();
            }
            
            // Configure components
            _gameTimer.preparationTime = preparationTime;
            _gameTimer.gameTime = gameTime;
            _playerSessionManager.maxPlayers = maxPlayers;
            
            // Subscribe to events
            _gameTimer.OnTimeUpdated += HandleTimeUpdated;
            _playerSessionManager.OnPlayersUpdated += HandlePlayersUpdated;
        }
        
        /// <summary>
        /// Establece el estado inicial del juego.
        /// Configura el estado Waiting y inicializa componentes hijos.
        /// </summary>
        private void SetInitialState()
        {
            _currentState = GameState.Waiting;
            _gameTimer.Initialize();
            _playerSessionManager.Initialize();
        }
        
        /// <summary>
        /// Actualiza el estado del juego cada frame.
        /// Procesa temporizador solo durante estados activos.
        /// </summary>
        /// <remarks>
        /// Solo actualiza el temporizador durante Preparing y Playing.
        /// Verifica si el tiempo ha expirado para cambiar de estado.
        /// </remarks>
        private void Update()
        {
            bool shouldUpdateTimer = _currentState == GameState.Preparing || _currentState == GameState.Playing;
            if (shouldUpdateTimer)
            {
                _gameTimer.UpdateTimer();
                
                if (_gameTimer.IsTimeExpired())
                {
                    HandleTimeExpired();
                }
            }
        }
        
        /// <summary>
        /// Maneja el evento de actualización de tiempo del temporizador.
        /// Propaga el evento a los suscriptores externos.
        /// </summary>
        /// <param name="remainingTime">Tiempo restante en segundos</param>
        private void HandleTimeUpdated(float remainingTime)
        {
            OnTimeUpdated?.Invoke(remainingTime);
        }
        
        /// <summary>
        /// Maneja el evento de actualización de jugadores.
        /// Propaga el evento a los suscriptores externos.
        /// </summary>
        /// <param name="playerCount">Cantidad actual de jugadores conectados</param>
        private void HandlePlayersUpdated(int playerCount)
        {
            OnPlayersUpdated?.Invoke(playerCount);
        }
        
        /// <summary>
        /// Maneja la expiración del tiempo del temporizador.
        /// Determina el siguiente estado según el estado actual.
        /// </summary>
        /// <remarks>
        /// De Preparing pasa a Playing, de Playing pasa a Finished.
        /// </remarks>
        private void HandleTimeExpired()
        {
            GameState nextState = _currentState == GameState.Preparing ? GameState.Playing : GameState.Finished;
            ChangeState(nextState);
        }
        
        /// <summary>
        /// Inicia el ciclo de juego si está en estado Waiting.
        /// Transiciona a Preparing y comienza la cuenta regresiva.
        /// </summary>
        /// <remarks>
        /// Solo permite iniciar desde el estado Waiting.
        /// Registra advertencia si se intenta iniciar desde otro estado.
        /// </remarks>
        public void StartGame()
        {
            if (_currentState == GameState.Waiting)
            {
                ChangeState(GameState.Preparing);
                _gameTimer.SetPreparationTime();
                _gameTimer.StartTimer(preparationTime);
                
                Debug.Log("[GameManager] Starting game preparation");
            }
            else
            {
                Debug.LogWarning($"[GameManager] Cannot start game - current state: {_currentState}");
            }
        }
        
        /// <summary>
        /// Finaliza la partida actual.
        /// Fuerza la transición al estado Finished.
        /// </summary>
        public void EndGame()
        {
            ChangeState(GameState.Finished);
            Debug.Log("[GameManager] Game finished");
        }
        
        /// <summary>
        /// Realiza la transición entre estados del juego.
        /// Configura comportamientos específicos de cada estado.
        /// </summary>
        /// <param name="newState">Nuevo estado al que transicionar</param>
        /// <remarks>
        /// Cada estado tiene configuraciones específicas de temporizador.
        /// Dispara el evento OnGameStateChanged al finalizar.
        /// </remarks>
        private void ChangeState(GameState newState)
        {
            GameState previousState = _currentState;
            _currentState = newState;
            
            switch (_currentState)
            {
                case GameState.Preparing:
                    _gameTimer.SetPreparationTime();
                    _gameTimer.StartTimer(preparationTime);
                    break;
                    
                case GameState.Playing:
                    _gameTimer.SetGameTime();
                    _gameTimer.StartTimer(gameTime);
                    break;
                    
                case GameState.Finished:
                    _gameTimer.StopTimer();
                    ProcessResults();
                    break;
            }
            
            OnGameStateChanged?.Invoke(_currentState);
            Debug.Log($"[GameManager] State changed: {previousState} -> {_currentState}");
        }
        
        /// <summary>
        /// Procesa los resultados finales de la partida.
        /// Calcula puntuaciones y determina ganadores.
        /// </summary>
        /// <remarks>
        /// Actualmente placeholder para futura implementación.
        /// Debe incluir lógica de puntuación y rankings.
        /// </remarks>
        private void ProcessResults()
        {
            // Game results processing logic
            Debug.Log("[GameManager] Processing final results");
        }
        
        /// <summary>
        /// Registra la conexión de un nuevo jugador.
        /// Notifica al gestor de sesiones del evento.
        /// </summary>
        public void PlayerConnected()
        {
            _playerSessionManager.PlayerConnected();
        }
        
        /// <summary>
        /// Registra la desconexión de un jugador.
        /// Notifica al gestor de sesiones del evento.
        /// </summary>
        public void PlayerDisconnected()
        {
            _playerSessionManager.PlayerDisconnected();
        }
        
        /// <summary>
        /// Obtiene el estado actual del juego.
        /// </summary>
        /// <returns>Estado actual del ciclo de vida del juego</returns>
        public GameState GetCurrentState()
        {
            return _currentState;
        }
        
        /// <summary>
        /// Obtiene el tiempo restante de la partida actual.
        /// </summary>
        /// <returns>Tiempo restante en segundos</returns>
        public float GetRemainingTime()
        {
            return _gameTimer.GetRemainingTime();
        }
        
        /// <summary>
        /// Obtiene la cantidad de jugadores actualmente conectados.
        /// </summary>
        /// <returns>Número de jugadores conectados</returns>
        public int GetConnectedPlayers()
        {
            return _playerSessionManager.GetConnectedPlayers();
        }
        
        /// <summary>
        /// Verifica si se pueden cumplir las condiciones para iniciar la partida.
        /// </summary>
        /// <returns>True si es posible iniciar el juego</returns>
        /// <remarks>
        /// Requiere estar en estado Waiting y cumplir condiciones del PlayerSessionManager.
        /// </remarks>
        public bool CanStartGame()
        {
            return _currentState == GameState.Waiting && _playerSessionManager.CanStartGame();
        }
    }
    
    /// <summary>
    /// Enumeración que representa los estados del ciclo de vida del juego.
    /// Define las fases por las que pasa una partida.
    /// </summary>
    public enum GameState
    {
        /// <summary>Esperando jugadores para iniciar la partida</summary>
        Waiting,
        /// <summary>Tiempo de preparación antes de comenzar</summary>
        Preparing,
        /// <summary>Partida en curso y activa</summary>
        Playing,
        /// <summary>Partida finalizada y procesando resultados</summary>
        Finished
    }
}
