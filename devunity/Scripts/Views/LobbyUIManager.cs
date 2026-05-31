using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using ChibitsLink.GameSide;
using ChibitsLink.Models;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.UI
{
    /// <summary>
    /// Gestor principal de la interfaz de usuario del lobby.
    /// Coordina todos los componentes UI del lobby y votaciones.
    /// </summary>
    /// <remarks>
    /// Maneja múltiples paneles UI y estados de conexión.
    /// Integra con LobbyManager y TcpNetworkServer.
    /// Proporciona interfaz para selección de red y votación de juegos.
    /// </remarks>
    public class LobbyUIManager : MonoBehaviour
    {
        [Header("Componentes de Red")]
        /// <summary>Gestor del lobby para conexión y gestión de jugadores</summary>
        public LobbyManager lobbyManager;
        /// <summary>Servidor TCP para comunicación con clientes móviles</summary>
        public TcpNetworkServer tcpServer;
        /// <summary>Personajes iniciales disponibles</summary>
        public Character[] initialCharacters;
        /// <summary>Juegos iniciales disponibles</summary>
        public Game[] initialGames;

        [Header("Paneles UI")]
        /// <summary>Panel de configuración inicial</summary>
        public GameObject setupPanel;
        /// <summary>Panel principal del lobby</summary>
        public GameObject lobbyPanel;

        [Header("Elementos UI")]
        /// <summary>Texto para mostrar código de sala</summary>
        public TextMeshProUGUI roomCodeText;
        /// <summary>Botón para iniciar lobby</summary>
        public Button startLobbyButton;
        /// <summary>Botón para regresar</summary>
        public Button backButton;
        /// <summary>Botón para salir</summary>
        public Button quitButton;
        /// <summary>Texto para mostrar estado</summary>
        public TextMeshProUGUI statusText;
        /// <summary>Texto para mostrar jugadores conectados</summary>
        public TextMeshProUGUI connectedPlayersText;

        [Header("Selección de Red")]
        /// <summary>Dropdown para seleccionar interfaz de red</summary>
        public TMP_Dropdown ipDropdown;
        /// <summary>Lista de interfaces de red disponibles</summary>
        private List<NetworkInterfaceData> _availableInterfaces;

        [Header("UI de Votación")]
        /// <summary>Panel para votación de juegos</summary>
        public GameObject votingPanel;
        /// <summary>Texto para temporizador de votación</summary>
        public TextMeshProUGUI votingTimerText;
        /// <summary>Duración de la votación</summary>
        public float votingDuration = 15f;

        /// <summary>Código de la sala actual</summary>
        private string _roomCode;
        /// <summary>Indica si hay votación en progreso</summary>
        private bool _isVoting = false;

        /// <summary>
        /// Se ejecuta al habilitar el componente.
        /// Refresca las interfaces de red disponibles.
        /// </summary>
        private void OnEnable()
        {
            try
            {
                RefreshNetworkInterfaces();
            }
            catch (ComponentNotFoundException ex)
            {
                Debug.LogError($"[LobbyUIManager] Failed to initialize: {ex.Message}");
                throw new ComponentNotFoundException("LobbyUIManager initialization failed", ex);
            }
            catch (NetworkServiceException ex)
            {
                Debug.LogError($"[LobbyUIManager] Failed to initialize: {ex.Message}");
                throw new NetworkServiceException("LobbyUIManager initialization failed", ex);
            }
            catch (System.NullReferenceException ex)
            {
                Debug.LogError($"[LobbyUIManager] Failed to initialize: {ex.Message}");
                throw new ComponentNotFoundException("Failed to initialize LobbyUIManager due to null reference", ex);
            }
        }

        /// <summary>
        /// Refresca la lista de interfaces de red disponibles.
        /// Actualiza el dropdown con las interfaces detectadas.
        /// </summary>
        public void RefreshNetworkInterfaces()
        {
            try
            {
                var manager = LobbyManager.Instance != null ? LobbyManager.Instance : lobbyManager;
                
                if (manager == null)
                {
                    throw new ComponentNotFoundException("LobbyManager not found");
                }

                _availableInterfaces = manager.GetAvailableNetworkInterfaces();
                
                if (ipDropdown != null)
                {
                    ipDropdown.ClearOptions();
                    
                    foreach (var networkInterface in _availableInterfaces)
                    {
                        ipDropdown.options.Add(new TMP_Dropdown.OptionData(networkInterface.ToString()));
                    }
                    
                    if (_availableInterfaces.Count > 0)
                    {
                        ipDropdown.value = 0;
                    }
                }
            }
            catch (ComponentNotFoundException ex)
            {
                Debug.LogError($"[LobbyUIManager] LobbyManager not found: {ex.Message}");
                throw new ComponentNotFoundException("LobbyUIManager failed to refresh network interfaces", ex);
            }
            catch (System.Net.NetworkInformation.NetworkInformationException ex)
            {
                Debug.LogError($"[LobbyUIManager] Failed to get network interfaces: {ex.Message}");
                throw new NetworkServiceException("Failed to refresh network interfaces", ex);
            }
            catch (System.NullReferenceException ex)
            {
                Debug.LogError($"[LobbyUIManager] Null reference during network refresh: {ex.Message}");
                throw new NetworkServiceException("Failed to refresh network interfaces due to null reference", ex);
            }
        }

        public void RefreshInterfaces()
        {
            var mgr = LobbyManager.Instance != null ? LobbyManager.Instance : lobbyManager;
            if (mgr != null && ipDropdown != null)
            {
                _availableInterfaces = mgr.GetAvailableNetworkInterfaces();
                ipDropdown.ClearOptions();
                List<string> options = new List<string>();
                foreach (var i in _availableInterfaces) options.Add(i.ToString());
                ipDropdown.AddOptions(options);

                int bestIndex = 0;
                bool preferredFound = false;
                string name;
                bool isPreferredInterface;
                for (int i = 0; i < _availableInterfaces.Count; i++)
                {
                    name = _availableInterfaces[i].Name.ToLower();
                    isPreferredInterface = !name.Contains("virtual") && !name.Contains("vbox") && !name.Contains("vmware");
                    if (isPreferredInterface && !preferredFound)
                    {
                        bestIndex = i;
                        preferredFound = true;
                    }
                }
                ipDropdown.value = bestIndex;
            }
        }
        /// <summary>
        /// Inicialización de los listeners de botones.
        /// Configura los eventos onClick para los botones de la UI.
        /// </summary>
        private void Start()
        {
            if (startLobbyButton != null)
                startLobbyButton.onClick.AddListener(OnCreateLobby);
            
            if (backButton != null)
                backButton.onClick.AddListener(OnBackToMenu);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitGame);

            // Intentar registrarse en el servidor persistente
            if (TcpServer.Instance != null)
            {
                TcpServer.Instance.lobbyUI = this;
                
                // PERSISTENCIA: Si ya hay un código en el servidor (venimos de un minijuego), 
                // saltar directamente al panel de Lobby
                string currentCode = TcpServer.Instance.GetRoomCode();
                if (!string.IsNullOrEmpty(currentCode))
                {
                    _roomCode = currentCode;
                    string ip = LobbyManager.Instance != null ? LobbyManager.Instance.manualIpOverride : "";
                    roomCodeText.text = $"{ip}\n{_roomCode}";
                    
                    if (setupPanel != null) setupPanel.SetActive(false);
                    if (lobbyPanel != null) lobbyPanel.SetActive(true);
                    if (votingPanel != null) votingPanel.SetActive(false);
                    
                    // REFRESCAR: Forzar actualización de la lista de jugadores al volver
                    TcpServer.Instance.RefreshUIPlayerList();
                    
                    StartListeningToLobby();
                }
            }

            bool hasExistingRoom = !string.IsNullOrEmpty(_roomCode);
            if (!hasExistingRoom)
            {
                // Poblar selector de IPs
                RefreshInterfaces();

                // Mostrar solo el panel de configuración al inicio si no hay sala previa
                if (setupPanel != null) setupPanel.SetActive(true);
                if (lobbyPanel != null) lobbyPanel.SetActive(false);
                if (votingPanel != null) votingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Maneja el evento de crear lobby.
        /// Inicia el servidor TCP y genera código de sala.
        /// </summary>
        /// <remarks>
        /// Este método se ejecuta cuando el usuario pulsa el botón de crear lobby.
        /// Inicia el servidor TCP y genera un código de sala único.
        /// </remarks>
        private async void OnCreateLobby()
        {
            Debug.Log("[LobbyUI] Botón pulsado, iniciando proceso...");
            
            var mgr = LobbyManager.Instance != null ? LobbyManager.Instance : lobbyManager;
            var svr = TcpServer.Instance != null ? TcpServer.Instance : tcpServer;

            if (mgr == null || svr == null) 
            {
                Debug.LogError("[LobbyUI] Error: No se encontró LobbyManager o TcpServer (instancia o local).");
            }
            else
            {
                if (statusText != null) statusText.text = "Esperando jugadores...";
                if (startLobbyButton != null) startLobbyButton.interactable = false;

                // Obtener IP seleccionada
                string overrideIp = null;
                if (ipDropdown != null && _availableInterfaces != null && ipDropdown.value >= 0 && ipDropdown.value < _availableInterfaces.Count)
                {
                    overrideIp = _availableInterfaces[ipDropdown.value].IpAddress;
                }

                var party = await mgr.CreateNewLobbyAsync("Mi Partida", 11000, overrideIp);
                _roomCode = party?.RoomCode;
                
                if (!string.IsNullOrEmpty(_roomCode))
                {
                    Debug.Log($"[LobbyUI] Lobby creado exitosamente: {_roomCode}");
                    roomCodeText.text = $"{party.IpAddress}\n{_roomCode}";
                    setupPanel.SetActive(false);
                    lobbyPanel.SetActive(true);
                    svr.SetRoomCode(_roomCode);
                    svr.StartServer(); // Iniciar servidor SOLAMENTE al crear la sala

                    // Poblar BBDD con personajes y juegos desde el Inspector
                    List<Character> charList = initialCharacters != null ? new List<Character>(initialCharacters) : null;
                    List<Game> gameList = initialGames != null ? new List<Game>(initialGames) : null;
                    await mgr.SeedDataAsync(charList, gameList);
                    
                    // Empezar a escuchar cambios en el Lobby
                    StartListeningToLobby();
                }
                else
                {
                    if (statusText != null) statusText.text = "Error al crear sala.";
                    if (startLobbyButton != null) startLobbyButton.interactable = true;
                }
            }
        }

        /// <summary>
        /// Maneja el evento de regresar al menú principal.
        /// Detiene el servidor y regresa a la escena principal.
        /// </summary>
        private void OnBackToMenu()
        {
            Debug.Log("[LobbyUI] Volviendo al menú. Cerrando servidor...");
            
            var svr = TcpServer.Instance != null ? TcpServer.Instance : tcpServer;
            if (svr != null)
            {
                svr.StopServer();
                svr.SetRoomCode(null);
            }

            _roomCode = null;
            _isVoting = false;

            // Limpiar UI
            if (roomCodeText != null) roomCodeText.text = "";
            if (statusText != null) statusText.text = "";
            if (connectedPlayersText != null) connectedPlayersText.text = "Sala Vacía\nEsperando jugadores...";
            if (startLobbyButton != null) startLobbyButton.interactable = true;

            // Intercambiar Paneles
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
            if (votingPanel != null) votingPanel.SetActive(false);
            if (setupPanel != null) setupPanel.SetActive(true);
        }

        /// <summary>
        /// Maneja el evento de salir del juego.
        /// Cierra la aplicación y regresa al sistema operativo.
        /// </summary>
        private void OnQuitGame()
        {
            Debug.Log("[LobbyUI] Saliendo del juego...");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Comienza a escuchar conexiones al lobby existente.
        /// Activa el panel de lobby y actualiza lista de jugadores.
        /// </summary>
        private void StartListeningToLobby()
        {
            // Nota: En una implementación ideal, LobbyManager expondría un evento.
            // Para simplificar esta integración, usaremos un loop de monitoreo o un listener directo.
            _ = MonitorLobbyState();
        }

        /// <summary>
        /// Monitorea el estado del lobby de forma asíncrona.
        /// Consulta Firestore para detectar cambios en el estado del juego.
        /// </summary>
        /// <returns>Task para monitoreo asíncrono</returns>
        private async System.Threading.Tasks.Task MonitorLobbyState()
        {
            bool keepMonitoring = !string.IsNullOrEmpty(_roomCode) && !_isVoting;
            Party party;
            while (keepMonitoring)
            {
                // Consultar Firestore cada 2 segundos
                var partyDoc = await Firebase.Firestore.FirebaseFirestore.DefaultInstance
                    .Collection("parties").Document(_roomCode).GetSnapshotAsync();
                
                if (partyDoc.Exists)
                {
                    party = partyDoc.ConvertTo<Party>();
                    if (party.GameState == "VOTING")
                    {
                        Debug.Log("[LobbyUI] Fase de VOTACIÓN detectada desde Firestore.");
                        StartVotingCountdown();
                        keepMonitoring = false;
                    }
                }

                if (keepMonitoring)
                {
                    await System.Threading.Tasks.Task.Delay(2000);
                    keepMonitoring = !string.IsNullOrEmpty(_roomCode) && !_isVoting;
                }
            }
        }

        /// <summary>
        /// Inicia la cuenta regresiva para votación.
        /// Muestra el panel de votación y temporizador.
        /// </summary>
        private async void StartVotingCountdown()
        {
            _isVoting = true;
            if (votingPanel != null) votingPanel.SetActive(true);
            
            float timer = votingDuration;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                if (votingTimerText != null) votingTimerText.text = $"Votando: {Mathf.CeilToInt(timer)}s";
                await System.Threading.Tasks.Task.Yield();
            }

            Debug.Log("[LobbyUI] Tiempo de votación terminado. Decidiendo juego...");
            var mgr = LobbyManager.Instance != null ? LobbyManager.Instance : lobbyManager;
            if (mgr != null) await mgr.DecideWinnerAndStartGameAsync(_roomCode);
        }

        /// <summary>
        /// Actualiza la lista de jugadores conectados.
        /// Muestra los nombres en la UI del lobby.
        /// </summary>
        /// <param name="names">Lista de nombres de jugadores conectados</param>
        public void UpdatePlayerList(List<string> names)
        {
            if (connectedPlayersText != null)
            {
                if (names == null || names.Count == 0)
                {
                    connectedPlayersText.text = "Sala Vacía\nEsperando jugadores...";
                }
                else
                {
                    connectedPlayersText.text = string.Join("\n", names);
                }
            }
        }
    }
}
