using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Firestore;
using ChibitsLink.Models;
using ChibitsLink.Utils;

namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Datos de usuario para el lobby.
    /// Contiene información básica del jugador.
    /// </summary>
    public class UserData
    {
        /// <summary>Nombre del usuario</summary>
        public string username = "Jugador";
        /// <summary>Nivel del usuario</summary>
        public int level = 1;
    }

    /// <summary>
    /// Datos de interfaz de red.
    /// Representa una interfaz de red con su IP.
    /// </summary>
    public class NetworkInterfaceData
    {
        /// <summary>Nombre de la interfaz</summary>
        public string Name;
        /// <summary>Dirección IP de la interfaz</summary>
        public string IpAddress;
        /// <summary>Representación en string</summary>
        public override string ToString() => $"{Name} ({IpAddress})";
    }

    /// <summary>
    /// Gestor principal de lobby y sincronización con la aplicación móvil.
    /// Maneja creación de salas, votaciones, puntuaciones y comunicación Firebase.
    /// </summary>
    /// <remarks>
    /// Implementa patrón Singleton para acceso global.
    /// Coordina entre Unity, Firebase y clientes móviles.
    /// Gestiona el ciclo de vida completo de las partidas.
    /// </remarks>
    /// <seealso cref="https://firebase.google.com/docs/firestore">
    /// Documentación de Firebase Firestore
    /// </seealso>
    {
        /// <summary>Instancia global del LobbyManager (patrón Singleton)</summary>
        public static LobbyManager Instance { get; private set; }
        
        /// <summary>Instancia de Firebase Firestore</summary>
        private FirebaseFirestore _firestore;
        /// <summary>Indica si Firebase está inicializado</summary>
        private bool _isInitialized = false;
        /// <summary>Nombre de la colección de parties</summary>
        private const string LOBBY_COLLECTION = "parties";
        /// <summary>Nombre de la colección de personajes</summary>
        private const string CHARACTERS_COLLECTION = "characters";
        /// <summary>Nombre de la colección de juegos</summary>
        private const string GAMES_COLLECTION = "games";
        /// <summary>Generador de números aleatorios</summary>
        private static readonly System.Random _random = new System.Random();
        
        /// <summary>Gestor de jugadores (asignar en inspector)</summary>
        public PlayerManager playerManager;
        /// <summary>Lista de personajes iniciales</summary>
        public List<Character> initialCharacters;
        /// <summary>Lista de juegos iniciales</summary>
        public List<Game> initialGames;

        [Header("Configuración de Red")]
        /// <summary>Override manual de IP (si no está vacío)</summary>
        public string manualIpOverride = "";
        
        /// <summary>Puntuaciones de la sesión actual</summary>
        public Dictionary<string, int> SessionScores { get; private set; } = new Dictionary<string, int>();
        /// <summary>Juegos jugados en la sesión actual</summary>
        public List<string> SessionPlayedGames { get; private set; } = new List<string>();
        
        /// <summary>Estado actual del juego</summary>
        public string GameState { get; private set; } = "LOBBY";
        /// <summary>Votos de los jugadores (privado)</summary>
        private Dictionary<string, string> _playerVotes = new Dictionary<string, string>();
        /// <summary>Código de sala actual</summary>
        private string _currentRoomCode;
        /// <summary>Código de sala público</summary>
        public string RoomCode => _currentRoomCode;

        /// <summary>
        /// Inicializa el LobbyManager y establece el patrón Singleton.
        /// Configura Firebase y persiste entre escenas.
        /// </summary>
        async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // DontDestroyOnLoad solo funciona correctamente en objetos raíz.
                // Si LobbyManager está anidado, persistimos toda la rama.
                DontDestroyOnLoad(transform.root.gameObject);
                await InitializeFirebase();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Inicializa Firebase y verifica dependencias.
        /// </summary>
        private async Task InitializeFirebase()
        {
            var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                _firestore = FirebaseFirestore.DefaultInstance;
                _isInitialized = true;
                Debug.Log("[LobbyManager] Firebase Initialized correctly.");
            }
            else
            {
                Debug.LogError($"[LobbyManager] Could not resolve Firebase dependencies: {dependencyStatus}");
            }
        }

        /// <summary>
        /// Espera a que Firebase esté inicializado.
        /// </summary>
        private async Task EnsureInitialized()
        {
            while (!_isInitialized) await Task.Delay(100);
        }

        /// <summary>
        /// Siembra datos iniciales en Firebase (personajes y juegos).
        /// Evita sobreescribir datos existentes.
        /// </summary>
        /// <param name="characters">Lista de personajes a sembrar</param>
        /// <param name="games">Lista de juegos a sembrar</param>
        public async Task SeedDataAsync(List<Character> characters = null, List<Game> games = null)
        {
            await EnsureInitialized();
            
            Debug.Log($"[LobbyManager] DIAGNÓSTICO: Conectado a Proyecto: {_firestore.App.Options.ProjectId}");

            // 1. Obtener IDs existentes para evitar sobreescribir si el usuario no quiere
            var existingChars = await _firestore.Collection(CHARACTERS_COLLECTION).GetSnapshotAsync();
            var existingCharIds = existingChars.Documents.Select(d => d.Id).ToList();
            
            if (existingCharIds.Count > 0)
            {
                Debug.Log($"[LobbyManager] Se han encontrado {existingCharIds.Count} personajes existentes: {string.Join(", ", existingCharIds)}");
            }
            else
            {
                Debug.Log("[LobbyManager] La colección 'characters' está vacía en Firestore.");
            }

            if (characters == null || characters.Count == 0)
            {
                characters = new List<Character>();
                
                // Intentar sacar de PlayerManager primero
                if (PlayerManager.Instance != null)
                {
                    foreach (var id in PlayerManager.Instance.GetAllCharacterIds())
                    {
                        characters.Add(new Character { 
                            Id = id, 
                            Name = char.ToUpper(id[0]) + id.Substring(1), // Capitalize
                            Description = $"Personaje {id} listo para el sofá.",
                            ImageUrl = $"char_{id.ToLower()}.png" 
                        });
                    }
                }
            }

            if (characters.Count == 0)
            {
                Debug.LogWarning("[LobbyManager] No se han encontrado personajes en el Inspector ni en PlayerManager para sembrar.");
            }
            else
            {
                var batch = _firestore.StartBatch();
                int charsAdded = 0;
                foreach (var c in characters)
                {
                    if (!existingCharIds.Contains(c.Id))
                    {
                        var docRef = _firestore.Collection(CHARACTERS_COLLECTION).Document(c.Id);
                        batch.Set(docRef, c);
                        charsAdded++;
                    }
                }
            
                // 2. Sembrar Juegos
                var existingGames = await _firestore.Collection(GAMES_COLLECTION).GetSnapshotAsync();
                var existingGameIds = existingGames.Documents.Select(d => d.Id).ToList();
                int gamesAdded = 0;

                if (games == null || games.Count == 0)
                {
                    // Fallback a juegos por defecto si no hay nada en el inspector
                    var defaultGames = new List<Game> {
                        new Game { Id = "Minigame_Bomb", Name = "BombTag", Description = "¡Evita que te peguen la bomba!", ImageUrl = "bomb.png" },
                        new Game { Id = "Minigame_Coins", Name = "Recolección de Oro",    Description = "¡Sé el más rápido!",  ImageUrl = "combat_thumb.png" },
                        new Game { Id = "Minigame_HookParty", Name = "Hook Party",     Description = "¡Colúmpiate y no caigas!", ImageUrl = "hookparty_thumb.png" }
                    };
                    foreach (var g in defaultGames)
                    {
                        if (!existingGameIds.Contains(g.Id))
                        {
                            batch.Set(_firestore.Collection(GAMES_COLLECTION).Document(g.Id), g);
                            gamesAdded++;
                        }
                    }
                }
                else
                {
                    foreach (var g in games)
                    {
                        if (!existingGameIds.Contains(g.Id))
                        {
                            batch.Set(_firestore.Collection(GAMES_COLLECTION).Document(g.Id), g);
                            gamesAdded++;
                        }
                    }
                }

                if (charsAdded > 0 || gamesAdded > 0)
                {
                    await batch.CommitAsync();
                    Debug.Log($"[LobbyManager] SIEMBRA COMPLETADA: +{charsAdded} personajes, +{gamesAdded} juegos nuevos.");
                }
                else
                {
                    Debug.Log("[LobbyManager] SEMILLA: No hay datos nuevos para añadir. Todo está sincronizado.");
                }
            }
        }

        private string _currentRoomCode;
        public string RoomCode => _currentRoomCode;

        /// <summary>
        /// Crea un nuevo lobby y lo registra en Firebase.
        /// Genera código único y configura IP y puerto.
        /// </summary>
        /// <param name="lobbyName">Nombre del lobby</param>
        /// <param name="port">Puerto del servidor</param>
        /// <param name="overrideIp">IP override (opcional)</param>
        /// <returns>Party creado</returns>
        public async Task<Party> CreateNewLobbyAsync(string lobbyName, int port = 11000, string overrideIp = null)
        {
            await EnsureInitialized();
            
            // Siembra automática usando los datos del inspector si existen
            _ = SeedDataAsync(initialCharacters, initialGames);

            Debug.Log("[LobbyManager] Intentando crear lobby...");
            string roomCode = GenerateRoomCode();
            
            if (await IsCodeTaken(roomCode)) roomCode = GenerateRoomCode();
            _currentRoomCode = roomCode;
            SessionScores.Clear();

            // Determinar IP final
            string finalIp = !string.IsNullOrEmpty(overrideIp) ? overrideIp : GetLocalIPAddress();

            var newParty = new Party
            {
                Id = Guid.NewGuid().ToString(),
                Name = lobbyName,
                RoomCode = roomCode,
                PlayerIds = new List<string>(),
                IpAddress = finalIp,
                Port = port,
                CreatedAt = DateTime.UtcNow,
                PlayedGames = new List<string>(),
                ParticipantNames = new Dictionary<string, string>(),
                ParticipantCharacters = new Dictionary<string, string>(),
                PlayerScores = new Dictionary<string, int>()
            };

            await _firestore.Collection(LOBBY_COLLECTION)
                .Document(roomCode)
                .SetAsync(newParty);

            Debug.Log($"[LobbyManager] Lobby registrado en Firestore: {roomCode} | IP: {newParty.IpAddress} | Port: {newParty.Port}");
            return newParty;
        }

        private async void OnApplicationQuit()
        {
            if (!string.IsNullOrEmpty(_currentRoomCode) && _firestore != null)
            {
                // Intentar cerrar la sala al salir
                _ = CloseLobbyAsync(_currentRoomCode);
            }
        }

        /// <summary>
        /// Obtiene la dirección IP local para el servidor.
        /// Prioriza interfaces con gateway y excluye virtuales.
        /// </summary>
        /// <returns>Dirección IP local</returns>
        private string GetLocalIPAddress()
        {
            try 
            {
                // Estrategia 1: Buscar interfaces activas con Gateway (las más probables de ser la REAL)
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up && 
                        (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                    {
                        // Ignorar interfaces virtuales conocidas
                        string name = ni.Name.ToLower();
                        if (name.Contains("virtual") || name.Contains("wsl") || name.Contains("hyper-v") || name.Contains("vbox") || name.Contains("vmware"))
                            continue;

                        var props = ni.GetIPProperties();
                        if (props.GatewayAddresses.Count > 0)
                        {
                            foreach (var ip in props.UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    Debug.Log($"[LobbyManager] Detectada IP Principal (con Gateway): {ip.Address} en {ni.Name}");
                                    return ip.Address.ToString();
                                }
                            }
                        }
                    }
                }

                // Estrategia 2: Fallback a cualquier IP que no sea loopback ni virtual
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var validIps = host.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork && !ip.ToString().StartsWith("127."))
                    .ToList();

                // Intentar filtrar rangos de APIPA (169.254.x.x) y priorizar Hotspots
                var priorityIp = validIps.FirstOrDefault(ip => ip.ToString().StartsWith("192.168.43.") || ip.ToString().StartsWith("172.20.10."));
                if (priorityIp != null) return priorityIp.ToString();

                var finalIp = validIps.FirstOrDefault(ip => !ip.ToString().StartsWith("169.254."));
                return finalIp?.ToString() ?? (validIps.Count > 0 ? validIps[0].ToString() : "127.0.0.1");
            }
            catch (NetworkInformationException ex)
            {
                Debug.LogError($"[LobbyManager] Error de red obteniendo IP: {ex.Message}");
            }
            catch (SocketException ex)
            {
                Debug.LogError($"[LobbyManager] Error de socket obteniendo IP: {ex.Message}");
            }
            return "127.0.0.1";
        }

        /// <summary>
        /// Genera un código de lobby único de 6 dígitos.
        /// </summary>
        /// <returns>Código de lobby</returns>
        private string GenerateRoomCode()
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Obtiene todas las interfaces de red activas con sus IPs.
        /// </summary>
        /// <returns>Lista de interfaces disponibles</returns>
        public List<NetworkInterfaceData> GetAvailableNetworkInterfaces()
        {
            List<NetworkInterfaceData> interfaces = new List<NetworkInterfaceData>();
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        var props = ni.GetIPProperties();
                        foreach (var ip in props.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                interfaces.Add(new NetworkInterfaceData
                                {
                                    Name = ni.Name,
                                    IpAddress = ip.Address.ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (NetworkInformationException ex)
            {
                Debug.LogError($"[LobbyManager] Error de red listando interfaces: {ex.Message}");
            }
            return interfaces;
        }

        /// <summary>
        /// Verifica si un código de lobby ya está en uso.
        /// </summary>
        /// <param name="code">Código a verificar</param>
        /// <returns>True si está tomado</returns>
        private async Task<bool> IsCodeTaken(string code)
        {
            var doc = await _firestore.Collection(LOBBY_COLLECTION).Document(code).GetSnapshotAsync();
            return doc.Exists;
        }

        /// <summary>
        /// Cierra un lobby y guarda todos los datos finales.
        /// Marca como CLOSED y guarda puntuaciones atomicamente.
        /// </summary>
        /// <param name="roomCode">Código del lobby</param>
        /// <param name="names">Nombres de participantes</param>
        /// <param name="chars">Personajes de participantes</param>
        /// <param name="scores">Puntuaciones</param>
        /// <param name="levels">Niveles</param>
        /// <param name="playedGames">Juegos jugados</param>
        public async Task CloseLobbyAsync(
            string roomCode,
            Dictionary<string, string> names = null,
            Dictionary<string, string> chars = null,
            Dictionary<string, int> scores = null,
            Dictionary<string, int> levels = null,
            List<string> playedGames = null)
        {
            bool hasRoomCode = !string.IsNullOrEmpty(roomCode);
            if (hasRoomCode)
            {
                await EnsureInitialized();

                var docRef = _firestore.Collection(LOBBY_COLLECTION).Document(roomCode);

                // Escritura atómica: todos los datos o ninguno
                var updates = new Dictionary<string, object>
                {
                    { "GameState", "CLOSED" },
                    { "ParticipantNames",      names  ?? new Dictionary<string, string>() },
                    { "ParticipantCharacters", chars  ?? new Dictionary<string, string>() },
                    { "ParticipantLevels",      levels ?? new Dictionary<string, int>()    },
                    { "PlayerScores",          scores ?? new Dictionary<string, int>()    },
                    { "PlayedGames",           playedGames ?? new List<string>()          }
                };

                try
                {
                    await docRef.UpdateAsync(updates);
                    Debug.Log($"[LobbyManager] Sala {roomCode} cerrada con {scores?.Count ?? 0} puntuaciones guardadas.");
                    
                    // Limpiar bots al cerrar la lobby
                    if (PlayerManager.Instance != null)
                    {
                        PlayerManager.Instance.CleanupAllBots();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogError($"[LobbyManager] Error de operación inválida al cerrar sala {roomCode}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Actualiza la lista de participantes en tiempo real.
        /// Sincroniza con la aplicación móvil.
        /// </summary>
        /// <param name="roomCode">Código del lobby</param>
        /// <param name="updatedPlayerIds">IDs actualizados</param>
        /// <param name="names">Nombres</param>
        /// <param name="chars">Personajes</param>
        /// <param name="levels">Niveles</param>
        public async Task UpdateParticipantsAsync(string roomCode, List<string> updatedPlayerIds, Dictionary<string, string> names = null, Dictionary<string, string> chars = null, Dictionary<string, int> levels = null)
        {
            await EnsureInitialized();
            
            var docRef = _firestore.Collection(LOBBY_COLLECTION).Document(roomCode);
            var snapshot = await docRef.GetSnapshotAsync();
            
            var updates = new Dictionary<string, object>
            {
                { "PlayerIds", updatedPlayerIds }
            };

            if (names != null) updates.Add("ParticipantNames", names);
            if (chars != null) updates.Add("ParticipantCharacters", chars);
            if (levels != null) updates.Add("ParticipantLevels", levels);

            // Limpiar listos que se hayan ido
            if (snapshot.Exists)
            {
                var party = snapshot.ConvertTo<Party>();
                var newReady = party.ReadyPlayerIds.Where(id => updatedPlayerIds.Contains(id)).ToList();
                updates.Add("ReadyPlayerIds", newReady);

                // Cancelar VOTING si quedan 0 o 1 jugadores para evitar crash
                bool isSalaVacia = updatedPlayerIds.Count < 2;
                if ((party.GameState == "VOTING" || party.GameState == "IN_GAME") && isSalaVacia)
                {
                    Debug.LogWarning($"[LobbyManager] Solo {updatedPlayerIds.Count} jugador(es). Cancelando {party.GameState} -> LOBBY.");
                    updates["GameState"] = "LOBBY";
                    updates["Votes"] = new Dictionary<string, int>();
                }
            }

            await docRef.UpdateAsync(updates);
        }

        /// <summary>
        /// Cambia el estado de listo de un jugador.
        /// Activa votación cuando >50% están listos.
        /// </summary>
        /// <param name="roomCode">Código del lobby</param>
        /// <param name="userId">ID del jugador</param>
        /// <param name="isReady">Estado de listo</param>
        public async Task ToggleReadyAsync(string roomCode, string userId, bool isReady)
        {
            await EnsureInitialized();
            var docRef = _firestore.Collection(LOBBY_COLLECTION).Document(roomCode);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                var party = snapshot.ConvertTo<Party>();

                if (isReady)
                {
                    if (!party.ReadyPlayerIds.Contains(userId)) party.ReadyPlayerIds.Add(userId);
                }
                else
                {
                    party.ReadyPlayerIds.Remove(userId);
                }

                var updates = new Dictionary<string, object> { { "ReadyPlayerIds", party.ReadyPlayerIds } };

                // Activar votación si > 50% están listos.
                // Usamos Math.Max para cubrir la race condition en la que PlayerIds aún no se ha
                // sincronizado a Firestore cuando llega el primer READY.
                int knownPlayers = Math.Max(party.PlayerIds.Count, party.ReadyPlayerIds.Count);
                if (knownPlayers > 0 && party.ReadyPlayerIds.Count >= knownPlayers * 0.5f && party.GameState == "LOBBY")
                {
                    updates.Add("GameState", "VOTING");
                    GameState = "VOTING";
                    _playerVotes.Clear();
                    Debug.Log("[LobbyManager] Umbral del 50% alcanzado. Iniciando VOTACIÓN.");
                }

                await docRef.UpdateAsync(updates);
            }
        }

        /// <summary>
        /// Registra un voto para el juego a jugar.
        /// </summary>
        /// <param name="roomCode">Código del lobby</param>
        /// <param name="gameId">ID del juego votado</param>
        public async Task RegisterVoteAsync(string roomCode, string gameId)
        {
            await EnsureInitialized();
            var docRef = _firestore.Collection(LOBBY_COLLECTION).Document(roomCode);
            
            var updates = new Dictionary<string, object>
            {
                { $"Votes.{gameId}", FieldValue.Increment(1) }
            };

            await docRef.UpdateAsync(updates);
            
            // Opcional: Podríamos verificar aquí si todos han votado para terminar antes
            // Pero por ahora, el host (Unity) podría tener un botón de "Empezar" o un timer.
        }

        /// <summary>
        /// Decide el juego ganador por votación e inicia el juego.
        /// Cambia a IN_GAME y carga la escena del juego.
        /// </summary>
        /// <param name="roomCode">Código del lobby</param>
        public async Task DecideWinnerAndStartGameAsync(string roomCode)
        {
            await EnsureInitialized();
            var docRef = _firestore.Collection(LOBBY_COLLECTION).Document(roomCode);
            var snapshot = await docRef.GetSnapshotAsync();
            
            if (snapshot.Exists)
            {
                var party = snapshot.ConvertTo<Party>();

                // CONTAR VOTOS LOCALES (Los que llegaron por TCP)
                var localVotesCount = _playerVotes.Values
                    .GroupBy(v => v)
                    .ToDictionary(g => g.Key, g => g.Count());

                string winnerGameId = ResolveWinnerGame(localVotesCount);

            Debug.Log($"[LobbyManager] Ganador decidido por votos locales: {winnerGameId}.");
            
            var finalNames = TcpServer.Instance?.GetSessionNames() ?? new Dictionary<string, string>();
            var finalChars = TcpServer.Instance?.GetSessionChars() ?? new Dictionary<string, string>();

            if (party.PlayedGames == null) party.PlayedGames = new List<string>();
            party.PlayedGames.Add(winnerGameId);
            
            // También guardamos en nuestra lista global de sesión
            if (!SessionPlayedGames.Contains(winnerGameId))
                SessionPlayedGames.Add(winnerGameId);

            var updates = new Dictionary<string, object> 
            { 
                { "GameState",             "IN_GAME" },
                { "Votes",                 new Dictionary<string, int>() },
                { "PlayedGames",           party.PlayedGames },
                { "ParticipantNames",      finalNames },
                { "ParticipantCharacters", finalChars }
            };

            GameState = "IN_GAME";
            _playerVotes.Clear();

                await docRef.UpdateAsync(updates);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    SceneManager.LoadScene(winnerGameId);
                });
            }
        }

        /// <summary>
        /// Maneja un voto recibido desde un cliente móvil.
        /// </summary>
        /// <param name="uid">ID del usuario</param>
        /// <param name="gameId">ID del juego votado</param>
        public void HandleVote(string uid, string gameId)
        {
            // Permitir votos si estamos en LOBBY (empezando transición) o VOTING
            bool canReceiveVote = GameState == "VOTING" || GameState == "LOBBY";
            if (canReceiveVote)
            {
                string cleanId = gameId.Trim();
                _playerVotes[uid] = cleanId;
                Debug.Log($"[LobbyManager] Voto registrado: Jugador {uid} -> '{cleanId}'");
            }
        }

        /// <summary>
        /// Resuelve el juego ganador basado en los votos.
        /// Si no hay votos, selecciona aleatoriamente.
        /// </summary>
        /// <param name="votes">Diccionario de votos</param>
        /// <returns>ID del juego ganador</returns>
        private string ResolveWinnerGame(Dictionary<string, int> votes)
        {
            Debug.Log($"[LobbyManager] Resolviendo ganador entre {votes?.Count ?? 0} opciones de voto.");
            
            if (votes == null || votes.Count == 0)
            {
                if (initialGames != null && initialGames.Count > 0)
                {
                    var randomGame = initialGames[_random.Next(initialGames.Count)];
                    Debug.Log($"[LobbyManager] NO HAY VOTOS. Selección aleatoria: {randomGame.Id}");
                    return randomGame.Id;
                }
                return "Minigame_Bomb";
            }

            foreach(var v in votes) Debug.Log($"[LobbyManager] - {v.Key}: {v.Value} votos");

            int maxVotes = votes.Values.Max();
            var topGames = votes.Where(v => v.Value == maxVotes)
                                .Select(v => v.Key)
                                .ToList();

            string winner = topGames[_random.Next(topGames.Count)];
            Debug.Log($"[LobbyManager] ¡GANADOR! El juego elegido es: {winner}");
            return winner;
        }

        /// <summary>
        /// Actualiza la puntuación de un jugador en la sala.
        /// Incrementa puntos en Firestore y cache local.
        /// </summary>
        /// <param name="roomCode">Código del lobby</param>
        /// <param name="userId">ID del jugador</param>
        /// <param name="pointsToAdd">Puntos a añadir</param>
        public async Task UpdatePlayerScoreAsync(string roomCode, string userId, int pointsToAdd)
        {
            await EnsureInitialized();
            var docRef = _firestore.Collection(LOBBY_COLLECTION).Document(roomCode);

            // 1. Incrementar puntos en Firestore atomicamente
            await docRef.UpdateAsync(new Dictionary<string, object>
            {
                { $"PlayerScores.{userId}", FieldValue.Increment(pointsToAdd) }
            });

            // 2. Actualizar cache local de la sesión para la UI
            UpdateSessionScoreCache(userId, pointsToAdd);

            // NOTA: El XP al perfil del usuario se suma en FinalizePartyScoresAsync
            // al cerrar la sala, no punto por punto. Ver StopServer -> CloseLobbyAsync.
            Debug.Log($"[LobbyManager] {userId} +{pointsToAdd} pts en sala {roomCode}.");
        }

        /// <summary>
        /// Actualiza la cache local de puntuaciones para la UI.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="pointsToAdd">Puntos a añadir</param>
        private void UpdateSessionScoreCache(string userId, int pointsToAdd)
        {
            if (SessionScores.ContainsKey(userId))
                SessionScores[userId] += pointsToAdd;
            else
                SessionScores[userId] = pointsToAdd;
        }

        /// <summary>
        /// Regresa al estado de lobby.
        /// Cambia estado y notifica a clientes móviles.
        /// </summary>
        public void ReturnToLobby()
        {
            bool hasRoomCode = !string.IsNullOrEmpty(_currentRoomCode);
            if (hasRoomCode)
            {
                _ = ReturnToLobbyAsync(_currentRoomCode);
            }
        }

        /// <summary>
        /// Regresa al lobby de forma asíncrona.
        /// Actualiza Firestore y carga escena del menú.
        /// </summary>
        /// <param name="roomCode">Código del lobby</param>
        public async Task ReturnToLobbyAsync(string roomCode)
        {
            await EnsureInitialized();
            Debug.Log($"[LobbyManager] Volviendo al Lobby ({roomCode}).");
            
            var docRef = _firestore.Collection(LOBBY_COLLECTION).Document(roomCode);
            
            // 1. Cambiamos estado en Firestore a LOBBY y limpiamos listos PRIMERO
            // Esto asegura que cuando la App llegue al LobbyPage, el estado ya sea el correcto.
            var updates = new Dictionary<string, object>
            {
                { "GameState", "LOBBY" },
                { "ReadyPlayerIds", new List<string>() }
            };

            try
            {
                await docRef.UpdateAsync(updates);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LobbyManager] No se pudo actualizar estado a LOBBY en Firestore: {ex.Message}");
            }
            
            GameState = "LOBBY";

            // 2. Volver a la escena de Lobby primero
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                SceneManager.LoadScene("menu"); 
            });

            // 3. Esperar un poco y luego notificar a los mandos móviles
            await Task.Delay(500); // Dar tiempo a que la escena cargue
            if (TcpServer.Instance != null)
            {
                TcpServer.Instance.BroadcastToAll("GOTO_LOBBY");
            }
        }

        /// <summary>
        /// El XP al perfil del usuario ahora se suma desde la App MAUI al cerrar la sala.
        /// Método obsoleto - mantenido por compatibilidad.
        /// </summary>
        [System.Obsolete("Este método está obsoleto. El XP se maneja desde la App MAUI.")]
        public async Task FinalizePartyScoresAsync(string roomCode)
        {
            await Task.CompletedTask;
        }
        /// <summary>
        /// Obtiene los datos de un usuario desde Firebase.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Datos del usuario</returns>
        public async Task<UserData> FetchUserDataAsync(string userId)
        {
            var data = new UserData();
            if (string.IsNullOrWhiteSpace(userId)) return data;

            try
            {
                await EnsureInitialized();

                var snapshot = await _firestore
                    .Collection("users")
                    .Document(userId.Trim())
                    .GetSnapshotAsync();
                
                if (!snapshot.Exists) return data;

                // Firestore serializa con el mismo nombre que el modelo (PascalCase)
                if (snapshot.TryGetValue("Username", out string username))
                    data.username = username;

                if (snapshot.TryGetValue("Level", out int level))
                    data.level = level;
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogWarning($"[LobbyManager] FetchUserDataAsync({userId}) operación inválida: {ex.Message}");
            }
            return data;
        }

        /// <summary>
        /// Añade experiencia al perfil del usuario y recalcula nivel.
        /// Regla: 1 nivel cada 100 XP.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="xpToAdd">XP a añadir</param>
        public async Task AddUserExperienceAsync(string userId, int xpToAdd)
        {
            bool canUpdateExperience = !string.IsNullOrWhiteSpace(userId) && !userId.StartsWith("BOT_");
            if (canUpdateExperience)
            {
                try
                {
                    await EnsureInitialized();
                    var userRef = _firestore.Collection("users").Document(userId.Trim());

                    await _firestore.RunTransactionAsync(async transaction =>
                    {
                        DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(userRef);
                        int currentXP = 0;
                        int currentLevel = 1;

                        if (snapshot.Exists)
                        {
                            if (snapshot.TryGetValue("Experience", out int xp)) currentXP = xp;
                            if (snapshot.TryGetValue("Level", out int lvl)) currentLevel = lvl;
                        }

                        int newXP = currentXP + xpToAdd;
                        // Regla de progresión modular: 100 pts por nivel
                        int newLevel = (newXP / 100) + 1;

                        var updates = new Dictionary<string, object>
                        {
                            { "Experience", newXP },
                            { "Level", newLevel }
                        };

                        transaction.Update(userRef, updates);
                        Debug.Log($"[Progreso] {userId}: XP {currentXP}->{newXP} (Nivel {newLevel})");
                    });
                }
                catch (InvalidOperationException ex)
                {
                    Debug.LogError($"[LobbyManager] Error de operación inválida actualizando XP de {userId}: {ex.Message}");
                }
            }
        }
    }
}
