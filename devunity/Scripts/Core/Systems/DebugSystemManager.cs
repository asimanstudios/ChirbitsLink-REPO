using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ChibitsLink.Services;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.Core.Systems
{
    /// <summary>
    /// Controlador principal de depuración - refactorizado con SOLID y responsabilidad única.
    /// Delega a servicios especializados para diferentes áreas.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    /// <remarks>
    /// Centraliza todas las funciones de depuración del proyecto.
    /// Permite control de logs, bots y transiciones de escena.
    /// Deshabilitable completamente para producción.
    /// </remarks>
    public class DebugManager : MonoBehaviour
    {
        /// <summary>Instancia global del DebugManager (patrón Singleton)</summary>
        public static DebugManager Instance { get; private set; }
        
        [Header("Control Principal")]
        /// <summary>Si está desmarcado, TODA LA DEPURACIÓN ESTÁ DESHABILITADA</summary>
        [Tooltip("Si está desmarcado, TODA LA DEPURACIÓN ESTÁ DESHABILITADA (sin logs, sin bots, sin escenas).")]
        public bool isDebugModeActive = true;

        [Header("Transición de Escena")]
        /// <summary>Nombre de la escena a cargar</summary>
        [Tooltip("Nombre de la escena a cargar (ej: Minigame_Coins, Minigame_HookParty, menu)")]
        public string sceneToLoad = "Minigame_Coins";
        
        /// <summary>Forzar carga inmediata de escena</summary>
        [Tooltip("Marcar esta casilla para cargar la escena inmediatamente.")]
        public bool forceLoadScene = false;

        [Header("Sistema de Bots (Simulación de Jugadores)")]
        /// <summary>Número de bots a añadir</summary>
        [Tooltip("Número de bots a añadir cuando se presiona el botón.")]
        public int numberOfBotsToAdd = 1;
        
        /// <summary>Generar bots ahora</summary>
        [Tooltip("Marcar esta casilla para generar bots ahora.")]
        public bool spawnBotsNow = false;
        
        [Header("Filtro de Logs")]
        /// <summary>Módulos de log activos</summary>
        [Tooltip("Expandir esta lista para seleccionar QUÉ módulos imprimirán logs en la consola.")]
        public DebugModule activeLogModules = DebugModule.All;

        // Servicios
        /// <summary>Servicio de gestión de logs</summary>
        private DebugLogService _logService;
        /// <summary>Servicio de carga de escenas</summary>
        private SceneLoaderService _sceneService;
        /// <summary>Servicio de gestión de bots</summary>
        private BotService _botService;
        
        /// <summary>Indica si está inicializado</summary>
        private bool _isInitialized;

        /// <summary>
        /// Inicialización del gestor de depuración.
        /// Establece el patrón Singleton y configura servicios.
        /// </summary>
        private void Awake()
        {
            InitializeDebugManager();
        }
        
        /// <summary>
        /// Inicializa el gestor de depuración.
        /// Configura servicios y persistencia entre escenas.
        /// </summary>
        private void InitializeDebugManager()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeServices();
                _isInitialized = true;
                
                Debug.Log("[DebugManager] Initialized successfully");
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeServices()
        {
            _logService = new DebugLogService();
            _sceneService = new SceneLoaderService();
            _botService = new BotService();
            
            // Configure services
            _logService.SetActiveModules(activeLogModules);
            _sceneService.SetTargetScene(sceneToLoad);
            
            _botService = FindObjectOfType<BotService>();
            if (_botService == null)
            {
                var botServiceGO = new GameObject("BotService");
                _botService = botServiceGO.AddComponent<BotService>();
            }
            
            // Sincronizar configuración
            _logService.isDebugModeActive = isDebugModeActive;
            _logService.activeLogModules = activeLogModules;
        }

        private void Update()
        {
            if (_isInitialized && isDebugModeActive)
            {
                ProcessDebugActions();
                UpdateServicesConfiguration();
            }
        }
        
        private void ProcessDebugActions()
        {
            if (forceLoadScene)
            {
                forceLoadScene = false;
                LoadMinigameScene();
            }

            if (spawnBotsNow)
            {
                spawnBotsNow = false;
                SpawnBots();
            }
        }
        
        private void UpdateServicesConfiguration()
        {
            if (_logService != null)
            {
                _logService.isDebugModeActive = isDebugModeActive;
                _logService.activeLogModules = activeLogModules;
            }
        }

        [ContextMenu("Cargar Escena Destino")]
        public void LoadMinigameScene()
        {
            if (_isInitialized && isDebugModeActive)
            {
                try
                {
                    _logService?.Log(DebugModule.General, $"Transicionando a la escena: {sceneToLoad}");
                    _sceneService?.LoadScene(sceneToLoad);
                }
                catch (SceneLoaderException ex)
                {
                    _logService?.LogError(DebugModule.General, $"Error cargando escena: {ex.Message}");
                }
            }
        }

        [ContextMenu("Generar Bots Ahora")]
        public void SpawnBots()
        {
            if (_isInitialized && isDebugModeActive)
            {
                try
                {
                    int spawnedCount = _botService?.SpawnBots(numberOfBotsToAdd) ?? 0;
                    _logService?.Log(DebugModule.Player, $"Bots generados: {spawnedCount}/{numberOfBotsToAdd}");
                }
                catch (BotServiceException ex)
                {
                    _logService?.LogError(DebugModule.Player, $"Error generando bots: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Métodos estáticos de compatibilidad - delegan al servicio de logs
        /// </summary>
        public static void Log(DebugModule module, string message)
        {
            Instance?._logService?.Log(module, message);
        }

        public static void LogWarning(DebugModule module, string message)
        {
            Instance?._logService?.LogWarning(module, message);
        }

        public static void LogError(DebugModule module, string message)
        {
            Instance?._logService?.LogError(module, message);
        }
        
        // Métodos de conveniencia para acceso directo a servicios
        public void RemoveAllBots()
        {
            if (_isInitialized && isDebugModeActive)
            {
                try
                {
                    _botService?.RemoveAllBots();
                    _logService?.Log(DebugModule.Player, "Todos los bots removidos");
                }
                catch (BotServiceException ex)
                {
                    _logService?.LogError(DebugModule.Player, $"Error removiendo bots: {ex.Message}");
                }
            }
        }
        
        public void ReloadCurrentScene()
        {
            if (_isInitialized && isDebugModeActive)
            {
                try
                {
                    _sceneService?.ReloadCurrentScene();
                }
                catch (SceneLoaderException ex)
                {
                    _logService?.LogError(DebugModule.General, $"Error recargando escena: {ex.Message}");
                }
            }
        }
        
        public int GetActiveBotCount()
        {
            return _isInitialized ? _botService?.GetActiveBotCount() ?? 0 : 0;
        }
        
        public bool IsSceneLoading()
        {
            return _isInitialized ? _sceneService?.IsLoading() ?? false : false;
        }
        
        private void OnDestroy()
        {
            // Limpieza de servicios si es necesario
            _isInitialized = false;
        }
    }
}
