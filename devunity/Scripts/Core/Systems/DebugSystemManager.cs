using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ChibitsLink.Services;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.Core.Systems
{
    /// <summary>
    /// Main debug controller - refactored with SOLID and single responsibility.
    /// Now delegates to specialized services.
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance { get; private set; }
        
        [Header("Main Control")]
        [Tooltip("If unchecked, ALL DEBUGGING IS DISABLED (no logs, no bots, no scenes).")]
        public bool isDebugModeActive = true;

        [Header("Scene Transition")]
        [Tooltip("Name of the scene to load (e.g., Minigame_Coins, Minigame_HookParty, menu)")]
        public string sceneToLoad = "Minigame_Coins";
        
        [Tooltip("Check this box to load the scene immediately.")]
        public bool forceLoadScene = false;

        [Header("Bot System (Player Simulation)")]
        [Tooltip("Number of bots to add when button is pressed.")]
        public int numberOfBotsToAdd = 1;
        
        [Tooltip("Check this box to generate bots now.")]
        public bool spawnBotsNow = false;
        
        [Header("Log Filter")]
        [Tooltip("Expand this list to select WHICH modules will print logs to console.")]
        public DebugModule activeLogModules = DebugModule.All;

        // Services
        private DebugLogService _logService;
        private SceneLoaderService _sceneService;
        private BotService _botService;
        
        private bool _isInitialized;

        private void Awake()
        {
            InitializeDebugManager();
        }
        
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
