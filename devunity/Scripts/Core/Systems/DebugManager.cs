using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ChibiCocina.Services;
using ChibiCocina.Core.Exceptions;

namespace ChibiCocina.Core
{
    /// <summary>
    /// Controlador principal de depuración - refactorizado con SOLID y responsabilidad única.
    /// Ahora delega a servicios especializados.
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        public static DebugManager Instance { get; private set; }
        
        [Header("Control Principal")]
        [Tooltip("Si se desmarca, SE DESACTIVA TODA LA DEPURACIÓN (ni logs, ni bots, ni escenas).")]
        public bool isDebugModeActive = true;

        [Header("Transición de Escenas")]
        [Tooltip("Nombre de la escena a la que quieres ir (ej: Minigame_Coins, Minigame_HookParty, menu)")]
        public string sceneToLoad = "Minigame_Coins";
        
        [Tooltip("Marca esta casilla para cargar la escena inmediatamente.")]
        public bool forceLoadScene = false;

        [Header("Sistema de Bots (Simulación de Jugadores)")]
        [Tooltip("Cantidad de bots a añadir al pulsar el botón.")]
        public int numberOfBotsToAdd = 1;
        
        [Tooltip("Marca esta casilla para generar los bots ahora.")]
        public bool spawnBotsNow = false;
        
        [Header("Filtro de Logs")]
        [Tooltip("Despliega esta lista para seleccionar QUÉ módulos imprimirán logs en la consola.")]
        public DebugModule activeLogModules = DebugModule.All;

        // Servicios
        private DebugLogService _logService;
        private SceneLoaderService _sceneService;
        private BotService _botService;
        
        private bool _isInitialized;

        private void Awake()
        {
            try
            {
                InitializeDebugManager();
            }
            catch (System.Exception ex)
            {
                throw new DebugServiceException("Error inicializando DebugManager", ex);
            }
        }
        
        private void InitializeDebugManager()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeServices();
                _isInitialized = true;
                
                Debug.Log("[DebugManager] Inicializado correctamente");
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeServices()
        {
            _logService = FindObjectOfType<DebugLogService>();
            if (_logService == null)
            {
                var logServiceGO = new GameObject("DebugLogService");
                _logService = logServiceGO.AddComponent<DebugLogService>();
            }
            
            _sceneService = FindObjectOfType<SceneLoaderService>();
            if (_sceneService == null)
            {
                var sceneServiceGO = new GameObject("SceneLoaderService");
                _sceneService = sceneServiceGO.AddComponent<SceneLoaderService>();
            }
            
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
            if (!_isInitialized || !isDebugModeActive) return;
            
            ProcessDebugActions();
            UpdateServicesConfiguration();
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
            if (!_isInitialized || !isDebugModeActive) return;
            
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

        [ContextMenu("Generar Bots Ahora")]
        public void SpawnBots()
        {
            if (!_isInitialized || !isDebugModeActive) return;
            
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
            if (!_isInitialized || !isDebugModeActive) return;
            
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
        
        public void ReloadCurrentScene()
        {
            if (!_isInitialized || !isDebugModeActive) return;
            
            try
            {
                _sceneService?.ReloadCurrentScene();
            }
            catch (SceneLoaderException ex)
            {
                _logService?.LogError(DebugModule.General, $"Error recargando escena: {ex.Message}");
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
