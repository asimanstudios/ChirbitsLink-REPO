using UnityEngine;
using ChibiCocina.Core;

namespace ChibiCocina.Core
{
    /// <summary>
    /// Módulos de depuración para filtrado de logs.
    /// Permite activar/desactivar logs por categoría.
    /// </summary>
    public enum DebugModule
    {
        /// <summary>Logs generales</summary>
        General = 1 << 0,
        /// <summary>Logs de red</summary>
        Network = 1 << 1,
        /// <summary>Logs de lobby</summary>
        Lobby = 1 << 2,
        /// <summary>Logs de jugadores</summary>
        Player = 1 << 3,
        /// <summary>Logs de minijuegos</summary>
        Minigame = 1 << 4,
        /// <summary>Logs de base de datos</summary>
        Database = 1 << 5,
        /// <summary>Todos los módulos</summary>
        All = ~0
    }
}

namespace ChibiCocina.Services
{
    /// <summary>
    /// Servicio de gestión de logs con filtrado por módulos.
    /// Centraliza el control de mensajes de depuración.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    /// <remarks>
    /// Permite deshabilitar logs por módulo específico.
    /// Esencial para desarrollo y depuración.
    /// Se puede desactivar completamente para producción.
    /// </remarks>
    public class DebugLogService : MonoBehaviour
    {
        /// <summary>Instancia global del servicio (patrón Singleton)</summary>
        public static DebugLogService Instance { get; private set; }
        
        [Header("Configuración de Logs")]
        /// <summary>Módulos de log activos</summary>
        public DebugModule activeLogModules = DebugModule.All;
        /// <summary>Indica si el modo debug está activo</summary>
        public bool isDebugModeActive = true;
        
        /// <summary>
        /// Inicialización del servicio.
        /// Establece el patrón Singleton y persistencia.
        /// </summary>
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
        
        /// <summary>
        /// Registra un mensaje de log si el módulo está activo.
        /// </summary>
        /// <param name="module">Módulo del log</param>
        /// <param name="message">Mensaje a registrar</param>
        public void Log(DebugModule module, string message)
        {
            if (CanLog(module))
            {
                Debug.Log($"[Debug | {module}] {message}");
            }
        }
        
        /// <summary>
        /// Registra una advertencia si el módulo está activo.
        /// </summary>
        /// <param name="module">Módulo del log</param>
        /// <param name="message">Mensaje a registrar</param>
        public void LogWarning(DebugModule module, string message)
        {
            if (CanLog(module))
            {
                Debug.LogWarning($"[Debug | {module}] {message}");
            }
        }
        
        public void LogError(DebugModule module, string message)
        {
            if (CanLog(module))
            {
                Debug.LogError($"[Debug | {module}] {message}");
            }
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
