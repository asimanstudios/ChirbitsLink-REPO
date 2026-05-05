using UnityEngine;
using ChibiCocina.Core;

namespace ChibiCocina.Core
{
    public enum DebugModule
    {
        General = 1 << 0,
        Network = 1 << 1,
        Lobby = 1 << 2,
        Player = 1 << 3,
        Minigame = 1 << 4,
        Database = 1 << 5,
        All = ~0
    }
}

namespace ChibiCocina.Services
{
    public class DebugLogService : MonoBehaviour
    {
        public static DebugLogService Instance { get; private set; }
        
        [Header("Configuración de Logs")]
        public DebugModule activeLogModules = DebugModule.All;
        public bool isDebugModeActive = true;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void Log(DebugModule module, string message)
        {
            if (!CanLog(module)) return;
            
            Debug.Log($"[Debug | {module}] {message}");
        }
        
        public void LogWarning(DebugModule module, string message)
        {
            if (!CanLog(module)) return;
            
            Debug.LogWarning($"[Debug | {module}] {message}");
        }
        
        public void LogError(DebugModule module, string message)
        {
            if (!CanLog(module)) return;
            
            Debug.LogError($"[Debug | {module}] {message}");
        }
        
        private bool CanLog(DebugModule module)
        {
            return isDebugModeActive && Instance != null && (activeLogModules & module) != 0;
        }
        
        public void SetActiveModules(DebugModule modules)
        {
            activeLogModules = modules;
        }
        
        public void ToggleDebugMode()
        {
            isDebugModeActive = !isDebugModeActive;
        }
        
        public bool IsModuleActive(DebugModule module)
        {
            return (activeLogModules & module) != 0;
        }
    }
}
