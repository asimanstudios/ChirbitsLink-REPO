using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using ChibitsLink.GameSide;
using ChibitsLink.Models;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.UI
{
    public class LobbyUIManager : MonoBehaviour
    {
        [Header("Networking Components")]
        public LobbyManager lobbyManager;
        public TcpNetworkServer tcpServer;
        public Character[] initialCharacters;
        public Game[] initialGames;

        [Header("UI Panels")]
        public GameObject setupPanel;
        public GameObject lobbyPanel;

        [Header("UI Elements")]
        public TextMeshProUGUI roomCodeText;
        public Button startLobbyButton;
        public Button backButton;
        public Button quitButton;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI connectedPlayersText;

        [Header("Network Selection")]
        public TMP_Dropdown ipDropdown;
        private List<NetworkInterfaceData> _availableInterfaces;

        [Header("Voting UI")]
        public GameObject votingPanel;
        public TextMeshProUGUI votingTimerText;
        public float votingDuration = 15f;

        private string _roomCode;
        private bool _isVoting = false;

        private void OnEnable()
        {
            try
            {
                RefreshNetworkInterfaces();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LobbyUIManager] Failed to initialize: {ex.Message}");
                throw new ComponentNotFoundException("Failed to initialize LobbyUIManager", ex);
            }
        }

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
            catch (System.Exception ex)
            {
                Debug.LogError($"[LobbyUIManager] Failed to refresh network interfaces: {ex.Message}");
                throw new NetworkServiceException("Failed to refresh network interfaces", ex);
            }
        }
            if (mgr != null && ipDropdown != null)
            {
                _availableInterfaces = mgr.GetAvailableNetworkInterfaces();
                ipDropdown.ClearOptions();
                List<string> options = new List<string>();
                foreach (var i in _availableInterfaces) options.Add(i.ToString());
                ipDropdown.AddOptions(options);

                int bestIndex = 0;
                bool preferredFound = false;
                for (int i = 0; i < _availableInterfaces.Count; i++)
                {
                    string name = _availableInterfaces[i].Name.ToLower();
                    bool isPreferredInterface = !name.Contains("virtual") && !name.Contains("vbox") && !name.Contains("vmware");
                    if (isPreferredInterface && !preferredFound)
                    {
                        bestIndex = i;
                        preferredFound = true;
                    }
                }
                ipDropdown.value = bestIndex;
            }
        }
        void Start()
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

        private void OnQuitGame()
        {
            Debug.Log("[LobbyUI] Saliendo del juego...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void StartListeningToLobby()
        {
            // Nota: En una implementación ideal, LobbyManager expondría un evento.
            // Para simplificar esta integración, usaremos un loop de monitoreo o un listener directo.
            _ = MonitorLobbyState();
        }

        private async System.Threading.Tasks.Task MonitorLobbyState()
        {
            bool keepMonitoring = !string.IsNullOrEmpty(_roomCode) && !_isVoting;
            while (keepMonitoring)
            {
                // Consultar Firestore cada 2 segundos
                var partyDoc = await Firebase.Firestore.FirebaseFirestore.DefaultInstance
                    .Collection("parties").Document(_roomCode).GetSnapshotAsync();
                
                if (partyDoc.Exists)
                {
                    var party = partyDoc.ConvertTo<Party>();
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
