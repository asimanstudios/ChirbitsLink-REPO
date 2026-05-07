using UnityEngine;
using Unity.Netcode;
using ChibiCocina.Models;

namespace ChibitsLink.Core
{
    /// <summary>
    /// Gestor de configuración de pantalla dividida para multijugador local.
    /// Ajusta cámaras y viewport según la cantidad de jugadores conectados.
    /// Implementa patrón Singleton para acceso global al sistema de pantalla.
    /// </summary>
    /// <remarks>
    /// Soporta configuraciones para 1-4 jugadores con diferentes distribuciones.
    /// Permite cambiar dinámicamente entre modos de pantalla.
    /// </remarks>
    public class SplitScreenManager : MonoBehaviour
    {
        /// <summary>Instancia global del gestor de pantalla dividida (patrón Singleton)</summary>
        public static SplitScreenManager Instance { get; private set; }
        
        [Header("Split Screen Configuration")]
        /// <summary>Array de cámaras para cada jugador (máximo 4)</summary>
        public Camera[] cameras;
        /// <summary>Áreas de UI para cada jugador (opcional)</summary>
        public RectTransform[] playerAreas;
        /// <summary>Indica si la pantalla dividida está activa</summary>
        public bool splitScreenActive;
        /// <summary>Modo actual de configuración de pantalla</summary>
        public SplitScreenMode currentMode;
        
        /// <summary>Número de jugadores actualmente activos</summary>
        private int _activePlayers;
        /// <summary>Configuraciones predefinidas para cada modo de pantalla</summary>
        private ScreenConfiguration[] _configurations;
        
        /// <summary>Evento disparado cuando cambia el modo de pantalla</summary>
        public System.Action<SplitScreenMode> OnModeChanged;
        /// <summary>Evento disparado cuando se actualiza la cantidad de jugadores</summary>
        public System.Action<int> OnPlayersUpdated;
        
        /// <summary>
        /// Inicializa el gestor y establece el patrón Singleton.
        /// Configura la pantalla dividida con valores iniciales.
        /// </summary>
        private void Awake()
        {
            InitializeSingleton();
        }
        
        /// <summary>
        /// Inicializa el patrón Singleton y configura el sistema.
        /// Asegura que solo exista una instancia del gestor.
        /// </summary>
        /// <remarks>
        /// Utiliza DontDestroyOnLoad para persistir entre escenas.
        /// Destruye instancias duplicadas automáticamente.
        /// </remarks>
        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSplitScreen();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Inicializa el sistema de pantalla dividida.
        /// Establece configuración inicial y prepara modos disponibles.
        /// </summary>
        private void InitializeSplitScreen()
        {
            _activePlayers = 1;
            currentMode = SplitScreenMode.SinglePlayer;
            splitScreenActive = false;
            
            InitializeConfigurations();
            ApplyConfiguration(currentMode);
            
            Debug.Log("[SplitScreenManager] Initialized");
        }
        
        /// <summary>
        /// Inicializa todas las configuraciones de pantalla disponibles.
        /// Crea configuraciones para 1, 2, 3 y 4 jugadores.
        /// </summary>
        private void InitializeConfigurations()
        {
            _configurations = new ScreenConfiguration[4];
            
            _configurations[0] = CreateSinglePlayerConfig();
            _configurations[1] = CreateTwoPlayerConfig();
            _configurations[2] = CreateThreePlayerConfig();
            _configurations[3] = CreateFourPlayerConfig();
        }
        
        /// <summary>
        /// Crea la configuración para modo de un solo jugador.
        /// </summary>
        /// <returns>Configuración de pantalla completa para un jugador</returns>
        private ScreenConfiguration CreateSinglePlayerConfig()
        {
            return new ScreenConfiguration
            {
                mode = SplitScreenMode.SinglePlayer,
                rectangles = new Rect[] { new Rect(0, 0, 1, 1) },
                activeCameras = new int[] { 0 }
            };
        }
        
        /// <summary>
        /// Crea la configuración para modo de dos jugadores horizontal.
        /// Divide la pantalla en dos mitades verticales.
        /// </summary>
        /// <returns>Configuración para dos jugadores en disposición horizontal</returns>
        private ScreenConfiguration CreateTwoPlayerConfig()
        {
            return new ScreenConfiguration
            {
                mode = SplitScreenMode.TwoPlayerHorizontal,
                rectangles = new Rect[] { 
                    new Rect(0, 0, 0.5f, 1), 
                    new Rect(0.5f, 0, 0.5f, 1) 
                },
                activeCameras = new int[] { 0, 1 }
            };
        }
        
        /// <summary>
        /// Crea la configuración para modo de tres jugadores.
        /// Dos jugadores en la parte superior, uno en la inferior central.
        /// </summary>
        /// <returns>Configuración para tres jugadores</returns>
        private ScreenConfiguration CreateThreePlayerConfig()
        {
            return new ScreenConfiguration
            {
                mode = SplitScreenMode.ThreePlayer,
                rectangles = new Rect[] { 
                    new Rect(0, 0.5f, 0.5f, 0.5f), 
                    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
                    new Rect(0.25f, 0, 0.5f, 0.5f)
                },
                activeCameras = new int[] { 0, 1, 2 }
            };
        }
        
        /// <summary>
        /// Crea la configuración para modo de cuatro jugadores.
        /// Divide la pantalla en cuatro cuadrantes iguales.
        /// </summary>
        /// <returns>Configuración para cuatro jugadores en cuadrantes</returns>
        private ScreenConfiguration CreateFourPlayerConfig()
        {
            return new ScreenConfiguration
            {
                mode = SplitScreenMode.FourPlayer,
                rectangles = new Rect[] { 
                    new Rect(0, 0.5f, 0.5f, 0.5f), 
                    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
                    new Rect(0, 0, 0.5f, 0.5f),
                    new Rect(0.5f, 0, 0.5f, 0.5f)
                },
                activeCameras = new int[] { 0, 1, 2, 3 }
            };
        }
        
        /// <summary>
        /// Actualiza la configuración de pantalla según la cantidad de jugadores.
        /// Cambia el modo si es necesario y aplica la configuración apropiada.
        /// </summary>
        /// <param name="playerCount">Cantidad de jugadores (1-4)</param>
        /// <remarks>
        /// Solo permite valores entre 1 y 4 jugadores.
        /// Dispara evento OnPlayersUpdated al finalizar.
        /// </remarks>
        public void UpdatePlayers(int playerCount)
        {
            bool isValidPlayerCount = playerCount >= 1 && playerCount <= 4;
            if (isValidPlayerCount)
            {
                _activePlayers = playerCount;
                SplitScreenMode newMode = GetModeForPlayers(playerCount);
                
                bool modeChanged = newMode != currentMode;
                if (modeChanged)
                {
                    ChangeMode(newMode);
                }
                else
                {
                    ApplySplitScreenConfiguration(newMode);
                }
            }
            
            OnPlayersUpdated?.Invoke(_activePlayers);
            Debug.Log($"[SplitScreenManager] Updated to {playerCount} players");
        }
        
        /// <summary>
        /// Determina el modo de pantalla apropiado según la cantidad de jugadores.
        /// </summary>
        /// <param name="players">Cantidad de jugadores</param>
        /// <returns>Modo de pantalla correspondiente</returns>
        /// <remarks>
        /// Utiliza expresión switch para mapeo directo.
        /// Por defecto retorna SinglePlayer para valores inválidos.
        /// </remarks>
        private SplitScreenMode GetModeForPlayers(int players)
        {
            return players switch
            {
                1 => SplitScreenMode.SinglePlayer,
                2 => SplitScreenMode.TwoPlayerHorizontal,
                3 => SplitScreenMode.ThreePlayer,
                4 => SplitScreenMode.FourPlayer,
                _ => SplitScreenMode.SinglePlayer
            };
        }
        
        /// <summary>
        /// Realiza el cambio de modo de pantalla.
        /// Aplica nueva configuración y dispara evento de cambio.
        /// </summary>
        /// <param name="newMode">Nuevo modo de pantalla a aplicar</param>
        private void ChangeMode(SplitScreenMode newMode)
        {
            currentMode = newMode;
            ApplyConfiguration(newMode);
            OnModeChanged?.Invoke(newMode);
            
            Debug.Log($"[SplitScreenManager] Mode changed to: {newMode}");
        }
        
        /// <summary>
        /// Aplica la configuración de pantalla para el modo especificado.
        /// Configura cámaras y viewports según el modo seleccionado.
        /// </summary>
        /// <param name="mode">Modo de pantalla a aplicar</param>
        /// <remarks>
        /// Deshabilita todas las cámaras antes de configurar las activas.
        /// Maneja errores si no se encuentra configuración válida.
        /// </remarks>
        private void ApplyConfiguration(SplitScreenMode mode)
        {
            ScreenConfiguration config;
            try
            {
                config = GetConfiguration(mode);
            }
            catch (System.ArgumentException ex)
            {
                Debug.LogError($"[SplitScreenManager] Failed to apply configuration: {ex.Message}");
            }
            
            DisableAllCameras();
            ConfigureActiveCameras(config);
            
            splitScreenActive = mode != SplitScreenMode.SinglePlayer;
        }
        
        /// <summary>
        /// Configura las cámaras activas según la configuración especificada.
        /// Establece viewport y habilita las cámaras necesarias.
        /// </summary>
        /// <param name="config">Configuración de pantalla a aplicar</param>
        /// <remarks>
        /// Valida que los índices de cámara estén dentro del rango.
        /// Solo configura cámaras que existan en el array.
        /// </remarks>
        private void ConfigureActiveCameras(ScreenConfiguration config)
        {
            int cameraCount = Mathf.Min(config.activeCameras.Length, config.rectangles.Length);
            
            for (int i = 0; i < cameraCount; i++)
            {
                int cameraIndex = config.activeCameras[i];
                Rect viewport = config.rectangles[i];
                
                bool isValidCamera = cameraIndex < cameras.Length && cameras[cameraIndex] != null;
                if (isValidCamera)
                {
                    cameras[cameraIndex].rect = viewport;
                    cameras[cameraIndex].enabled = true;
                }
            }
        }
        
        /// <summary>
        /// Deshabilita todas las cámaras del array.
        /// Utilizado antes de aplicar una nueva configuración.
        /// </summary>
        /// <remarks>
        /// Verifica nulidad del array antes de iterar.
        /// Solo deshabilita cámaras que no sean null.
        /// </remarks>
        private void DisableAllCameras()
        {
            if (cameras != null)
            {
            
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null)
                {
                    cameras[i].enabled = false;
                }
            }
        }
        
        /// <summary>
        /// Obtiene la configuración de pantalla para el modo especificado.
        /// </summary>
        /// <param name="mode">Modo de pantalla buscado</param>
        /// <returns>Configuración de pantalla correspondiente</returns>
        /// <exception cref="System.ArgumentException">Si no se encuentra configuración para el modo</exception>
        private ScreenConfiguration GetConfiguration(SplitScreenMode mode)
        {
            foreach (var config in _configurations)
            {
                if (config.mode == mode)
                    return config;
            }
            
            throw new System.ArgumentException($"No configuration found for mode: {mode}", nameof(mode));
        }
        
        /// <summary>
        /// Activa o desactiva el modo de pantalla dividida.
        /// Permite cambiar entre modo single y multijugador.
        /// </summary>
        /// <param name="enable">True para activar pantalla dividida</param>
        /// <remarks>
        /// Solo activa multijugador si hay más de un jugador.
        /// Si se desactiva, fuerza modo SinglePlayer.
        /// </remarks>
        public void SetSplitScreen(bool enable)
        {
            bool shouldEnableMultiplayer = enable && _activePlayers > 1;
            if (shouldEnableMultiplayer)
            {
                ChangeMode(GetModeForPlayers(_activePlayers));
            }
            else if (!enable)
            {
                ChangeMode(SplitScreenMode.SinglePlayer);
            }
        }
        
        /// <summary>
        /// Configura una cámara para un jugador específico.
        /// </summary>
        /// <param name="playerIndex">Índice del jugador (0-3)</param>
        /// <param name="camera">Cámara a asignar</param>
        /// <remarks>
        /// Valida que el índice esté dentro del rango del array.
        /// </remarks>
        public void ConfigureCamera(int playerIndex, Camera camera)
        {
            bool isValidIndex = playerIndex >= 0 && playerIndex < cameras.Length;
            if (isValidIndex)
            {
                cameras[playerIndex] = camera;
                Debug.Log($"[SplitScreenManager] Camera configured for player {playerIndex}");
            }
        }
        
        /// <summary>
        /// Obtiene la cámara configurada para un jugador específico.
        /// </summary>
        /// <param name="playerIndex">Índice del jugador (0-3)</param>
        /// <returns>Cámara configurada para el jugador</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Si el índice está fuera de rango</exception>
        /// <exception cref="System.InvalidOperationException">Si la cámara no está configurada</exception>
        public Camera GetPlayerCamera(int playerIndex)
        {
            bool isValidIndex = playerIndex >= 0 && playerIndex < cameras.Length;
            if (!isValidIndex)
            {
                throw new System.ArgumentOutOfRangeException(nameof(playerIndex), $"Player index {playerIndex} is out of range");
            }
            
            Camera camera = cameras[playerIndex];
            if (camera == null)
            {
                throw new System.InvalidOperationException($"Camera at index {playerIndex} is not configured");
            }
            
            return camera;
        }
        
        /// <summary>
        /// Obtiene el modo actual de configuración de pantalla.
        /// </summary>
        /// <returns>Modo de pantalla actual</returns>
        public SplitScreenMode GetCurrentMode()
        {
            return currentMode;
        }
        
        /// <summary>
        /// Obtiene la cantidad de jugadores actualmente activos.
        /// </summary>
        /// <returns>Número de jugadores activos</returns>
        public int GetActivePlayers()
        {
            return _activePlayers;
        }
        
        /// <summary>
        /// Verifica si la pantalla dividida está actualmente activa.
        /// </summary>
        /// <returns>True si la pantalla dividida está activa</returns>
        public bool IsSplitScreenActive()
        {
            return splitScreenActive;
        }
        
        /// <summary>
        /// Dibuja la interfaz de depuración en pantalla.
        /// Muestra información actual del sistema de pantalla dividida.
        /// </summary>
        /// <remarks>
        /// OnGUI es llamado automáticamente por Unity cada frame.
        /// </remarks>
        private void OnGUI()
        {
            DrawDebugUI();
        }
        
        /// <summary>
        /// Dibuja los elementos de la UI de depuración.
        /// Muestra estado actual y permite cambiar modos manualmente.
        /// </summary>
        /// <remarks>
        /// Solo visible durante desarrollo para facilitar pruebas.
        /// </remarks>
        private void DrawDebugUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
            GUILayout.Label($"Split Screen: {(splitScreenActive ? "Active" : "Inactive")}");
            GUILayout.Label($"Mode: {currentMode}");
            GUILayout.Label($"Players: {_activePlayers}");
            
            if (GUILayout.Button("Change Mode"))
            {
                HandleModeChangeButton();
            }
            
            GUILayout.EndArea();
        }
        
        /// <summary>
        /// Maneja el evento del botón de cambio de modo en la UI de depuración.
        /// Cicla through los modos disponibles de pantalla.
        /// </summary>
        /// <remarks>
        /// Utiliza aritmética modular para ciclar entre 0-3.
        /// </remarks>
        private void HandleModeChangeButton()
        {
            int nextMode = ((int)currentMode + 1) % 4;
            UpdatePlayers(nextMode + 1);
        }
    }
    
    /// <summary>
    /// Enumeración que representa los modos de configuración de pantalla dividida.
    /// Define cómo se distribuye la pantalla entre múltiples jugadores.
    /// </summary>
    public enum SplitScreenMode
    {
        /// <summary>Pantalla completa para un solo jugador</summary>
        SinglePlayer,
        /// <summary>Dos jugadores en división horizontal</summary>
        TwoPlayerHorizontal,
        /// <summary>Dos jugadores en división vertical</summary>
        TwoPlayerVertical,
        /// <summary>Tres jugadores en configuración especial</summary>
        ThreePlayer,
        /// <summary>Cuatro jugadores en cuadrantes</summary>
        FourPlayer
    }
    
    /// <summary>
    /// Clase que representa una configuración de pantalla específica.
    /// Almacena los rectángulos de viewport y cámaras activas.
    /// </summary>
    /// <remarks>
    /// Marcada como Serializable para poder editar en el inspector de Unity.
    /// </remarks>
    [System.Serializable]
    public class ScreenConfiguration
    {
        /// <summary>Modo de pantalla al que corresponde esta configuración</summary>
        public SplitScreenMode mode;
        /// <summary>Rectángulos de viewport para cada cámara activa</summary>
        public Rect[] rectangles;
        /// <summary>Índices de cámaras que deben estar activas</summary>
        public int[] activeCameras;
    }
}
