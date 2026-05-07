using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Versión FINAL con 2 LISTAS (Lobby y Juego), Singleton y Persistencia.
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }

        [Header("Configuración de Spawn")]
        public List<Transform> spawnPoints; 
        public ChibitsLink.UI.LobbyNotifications notifications; 
        
        [Header("Prefabs de Personajes")]
        public List<CharacterPrefabMap> lobbyCharacterPrefabs; 
        [FormerlySerializedAs("characterPrefabs")]
        public List<CharacterPrefabMap> gameCharacterPrefabs;

        [Serializable]
        public struct CharacterPrefabMap
        {
            public string characterId;
            public GameObject prefab;
            public Vector3 positionOffset; 
            public Vector3 rotationOffset; 
            public Vector3 localScale;     
        }

        private Dictionary<string, GameObject> _playerObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, string> _playerNames = new Dictionary<string, string>();
        private Dictionary<string, int> _playerLevels = new Dictionary<string, int>(); // Nuevo sistema de niveles
        private Dictionary<string, string> _playerLastCharId = new Dictionary<string, string>();
        private List<string> _connectionOrder = new List<string>(); 
        private bool isTransitioning = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(transform.root.gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
                FindSpawnPointsInScene();
            }
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) 
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                CleanupAllBots(); // Limpiar todos los bots al salir
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[PlayerManager] Escena '{scene.name}' cargada. Preparando spawn ordenado...");
            
            isTransitioning = true;
            _playerObjects.Clear();

            StartCoroutine(DelayedSpawnRoutine());
        }

        private System.Collections.IEnumerator DelayedSpawnRoutine()
        {
            // Esperar un frame para que los objetos marcados para Destroy de la escena anterior desaparezcan
            yield return null;

            // LIMPIEZA FÍSICA AGRESIVA: Eliminar cualquier residuo que tenga identidad de jugador en la nueva escena
            var residues = GameObject.FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);
            foreach (var r in residues)
            {
                Debug.Log($"[PlayerManager] Eliminando residuo encontrado: {r.gameObject.name}");
                Destroy(r.gameObject);
            }
            
            // Esperar otro frame para que la limpieza física se complete
            yield return null;

            FindSpawnPointsInScene();

            int idx = 0;
            foreach (var userId in _connectionOrder)
            {
                if (_playerLastCharId.TryGetValue(userId, out string charId))
                {
                    // Usar módulo para evitar solapamiento si hay más jugadores que spawn points
                    int spawnIndex = idx % spawnPoints.Count;
                    SpawnPlayer(userId, charId, spawnIndex);
                    idx++;
                }
            }

            isTransitioning = false;
            ConfigureCameras();
        }

        private void FindSpawnPointsInScene()
        {
            spawnPoints = new List<Transform>();
            var tagged = GameObject.FindGameObjectsWithTag("SpawnPoint");
            
            // MODULAR FIX: Sort by X position to fill "Left to Right" regardless of object name
            var sortedList = new List<GameObject>(tagged);
            sortedList.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
            
            foreach (var g in sortedList) spawnPoints.Add(g.transform);
            Debug.Log($"[PlayerManager] {spawnPoints.Count} spawn points encontrados (Ordenados de Izquierda a Derecha).");
        }

        public void HandlePlayerJoin(string userId, string charId, string username = "Jugador", int level = 1)
        {
            // Si ya existe este ID (ej: reconexión), mantener su posición original en _connectionOrder
            bool isReconnecting = _connectionOrder.Contains(userId);
            if (isReconnecting)
            {
                Debug.Log($"[PlayerManager] Re-unión detectada para UID: {userId}. Manteniendo posición original.");
                // No limpiar completamente, solo actualizar datos
                _playerNames[userId] = username;
                _playerLevels[userId] = level;
                _playerLastCharId[userId] = charId;
                
                // Solo spawnear si no estamos en medio de una transición
                if (!isTransitioning)
                {
                    int originalIndex = _connectionOrder.IndexOf(userId);
                    SpawnPlayer(userId, charId, originalIndex);
                    ConfigureCameras();
                }
                else
                {
                    Debug.LogWarning($"[PlayerManager] Cannot spawn player {userId} - transition in progress");
                }
                return;
            }

            // Nuevo jugador - añadir al final
            _connectionOrder.Add(userId);
            _playerNames[userId] = username;
            _playerLevels[userId] = level;
            if (notifications != null) notifications.ShowNotification($"{username} (Lvl {level}) se ha unido");
            
            _playerLastCharId[userId] = charId;

            // Si es un BOT, avisar al servidor TCP para que lo sincronice con Firestore
            if (userId.StartsWith("BOT_") && TcpServer.Instance != null)
            {
                TcpServer.Instance.RegisterBot(userId, charId, username, level);
            }

            // Solo spawnear si no estamos en medio de una transición de escena
            if (!isTransitioning)
            {
                SpawnPlayer(userId, charId, _connectionOrder.IndexOf(userId));
                ConfigureCameras();
            }
        }

        public void HandleCharacterSync(string userId, string newCharId)
        {
            _playerLastCharId[userId] = newCharId;
            if (_playerObjects.ContainsKey(userId))
            {
                SpawnPlayer(userId, newCharId, _connectionOrder.IndexOf(userId));
            }
        }

        private void SpawnPlayer(string userId, string charId, int playerIndex)
        {
            if (_playerObjects.TryGetValue(userId, out GameObject old)) Destroy(old);

            string sceneName = SceneManager.GetActiveScene().name.ToLower();
            bool isLobby = sceneName.Contains("lobby") || sceneName.Contains("menu");
            
            var list = isLobby ? lobbyCharacterPrefabs : gameCharacterPrefabs;
            var mapping = list.Find(m => m.characterId == charId);
            
            if (mapping.prefab == null && list.Count > 0) mapping = list[0];

            if (mapping.prefab != null)
            {
                // Safety: If more players than spawn points, wrap around or use last
                int spIndex = (spawnPoints.Count > 0) ? playerIndex % spawnPoints.Count : -1;
                Transform sp = (spIndex >= 0) ? spawnPoints[spIndex] : transform;
                
                // Instantiate in WORLD SPACE to avoid local coordinate issues
                GameObject player = Instantiate(mapping.prefab, sp.position, sp.rotation);
                
                // Apply offsets in world space if needed, or local if it's a child (we keep it as a child for hierarchy cleanup)
                player.transform.SetParent(sp);
                player.transform.localPosition = mapping.positionOffset;
                player.transform.localRotation = Quaternion.Euler(mapping.rotationOffset);
                player.transform.localScale = mapping.localScale == Vector3.zero ? Vector3.one : mapping.localScale;
                
                player.name = $"Player_{playerIndex + 1}_{userId}";
                
                var identity = player.AddComponent<PlayerIdentity>();
                identity.userId = userId;
                if (_playerNames.TryGetValue(userId, out string uname)) identity.username = uname;
                else identity.username = userId;
                if (_playerLevels.TryGetValue(userId, out int level)) identity.level = level;
                else identity.level = 1;

                _playerObjects[userId] = player;
                Debug.Log($"[PlayerManager] Spawneado {player.name} en index {playerIndex} (SP: {sp.name})");
            }
        }

        /// <summary>
        /// Detecta si la escena tiene cámara propia (aunque esté desactivada al inicio).
        /// Si la tiene → la activa y desactiva las de los prefabs de jugadores.
        /// Si no la tiene → activa solo la de P1, desactiva las del resto.
        /// </summary>
        private void ConfigureCameras()
        {
            // 1. Buscar TODAS las cámaras de la escena, incluidas las desactivadas
            var allCams = GameObject.FindObjectsOfType<Camera>(true); // true = includeInactive
            Camera sceneCamera = null;

            foreach (var cam in allCams)
            {
                if (cam.targetTexture != null) continue; // ignorar render textures (minimapas, etc.)

                // Comprobar si pertenece a algún jugador instanciado
                bool isPlayerCam = false;
                foreach (var playerObj in _playerObjects.Values)
                {
                    if (playerObj != null && cam.transform.IsChildOf(playerObj.transform))
                    {
                        isPlayerCam = true;
                    }
                }

                if (!isPlayerCam && sceneCamera == null)
                {
                    sceneCamera = cam;
                }
            }

            bool hasSceneCamera = sceneCamera != null;

            // 2. Si encontramos una cámara de escena, asegurarse de que está activa
            if (hasSceneCamera)
            {
                sceneCamera.gameObject.SetActive(true);
                Debug.Log($"[PlayerManager] Cámara de escena encontrada: {sceneCamera.name} — activada.");
            }

            // 3. Configurar cámaras de los prefabs de jugadores
            int idx = 0;
            foreach (var userId in _connectionOrder)
            {
                if (!_playerObjects.TryGetValue(userId, out GameObject playerObj) || playerObj == null)
                {
                    idx++;
                    continue;
                }

                // Solo P1 puede tener cámara cuando NO hay cámara de escena
                bool enableCam = !hasSceneCamera && idx == 0;

                var cam = playerObj.GetComponentInChildren<Camera>(true);
                if (cam != null) cam.gameObject.SetActive(enableCam);

                var listener = playerObj.GetComponentInChildren<AudioListener>(true);
                if (listener != null) listener.enabled = enableCam;

                idx++;
            }

            Debug.Log($"[PlayerManager] ConfigureCameras: cámara de escena={hasSceneCamera}");
        }

        public void HandleControllerInput(string userId, string json)
        {
            if (_playerObjects.TryGetValue(userId, out GameObject obj))
            {
                var controller = GetPlayerController(obj, userId);
                if (controller != null)
                {
                    ProcessInputForController(controller, userId, json);
                }
            }
            else
            {
                LogUnknownPlayer(userId);
            }
        }
        
        private IChibitsController GetPlayerController(GameObject obj, string userId)
        {
            // GetComponentInChildren busca en la raíz Y en todos los hijos (incluidos inactivos)
            // Necesario porque el controller suele estar en un nodo hijo del prefab
            var controller = obj.GetComponentInChildren<IChibitsController>(true);
            if (controller == null)
            {
                Debug.LogWarning($"[PlayerManager] El GameObject '{obj.name}' (userId={userId}) " +
                                 "no tiene ningún componente IChibitsController en su jerarquía. " +
                                 "Comprueba que el controller está en el prefab.");
            }
            return controller;
        }
        
        private void ProcessInputForController(IChibitsController controller, string userId, string json)
        {
            var input = JsonUtility.FromJson<TcpServer.ControllerInput>(json);
            if (input == null)
            {
                Debug.LogWarning($"[PlayerManager] JSON inválido para {userId}: {json}");
                return;
            }
            
            ProcessInputType(controller, input);
        }
        
        private void ProcessInputType(IChibitsController controller, TcpServer.ControllerInput input)
        {
            bool isJoystickInput = input.type == "joystick";
            bool isButtonInput = input.type == "button";

            if (isJoystickInput)
            {
                controller.ProcessJoystick(input.x, input.y);
            }
            else if (isButtonInput)
            {
                controller.ProcessButton(input.id, input.state);
            }
            else
            {
                Debug.LogWarning($"[PlayerManager] Tipo de input desconocido: '{input.type}'");
            }
        }
        
        private void LogUnknownPlayer(string userId)
        {
            string availableIds = _playerObjects.Count == 0 
                ? "(ninguno registrado)" 
                : string.Join(" | ", _playerObjects.Keys);
            Debug.LogWarning($"[PlayerManager] Input ignorado. UID de la App: '{userId}' — " +
                             $"UIDs registrados: {availableIds}");
        }

        public void HandlePlayerDisconnect(string userId)
        {
            Debug.Log($"[PlayerManager] Iniciando desconexión robusta para UID: {userId}");

            CleanupPlayerIdentity(userId);
            DestroyPlayerObject(userId);
            CleanupOrphanedIdentities(userId);
            
            ConfigureCameras();
            Debug.Log($"[PlayerManager] Desconexión completada para UID: {userId}");
        }
        
        private void CleanupPlayerIdentity(string userId)
        {
            // Limpiar rastro de IDENTIDAD (pero mantener posición en _connectionOrder para spawn consistente)
            if (_playerNames.TryGetValue(userId, out string name))
            {
                if (notifications != null) notifications.ShowNotification($"{name} ha salido");
                _playerNames.Remove(userId);
            }
            _playerLastCharId.Remove(userId);
            // NO eliminar de _connectionOrder para mantener spawn consistente al reconectar
        }
        
        private void DestroyPlayerObject(string userId)
        {
            // Destruir el objeto físico trackeado
            if (_playerObjects.TryGetValue(userId, out GameObject trackedObj))
            {
                if (trackedObj != null) 
                {
                    Debug.Log($"[PlayerManager] Destruyendo objeto trackeado para {userId}");
                    Destroy(trackedObj);
                }
                _playerObjects.Remove(userId);
            }
        }
        
        private void CleanupOrphanedIdentities(string userId)
        {
            // BUSQUEDA AGRESIVA DE HUÉRFANOS (Siempre, por seguridad)
            // Esto limpia personajes que se hayan quedado "sueltos" por cambios de escena o wipes parciales
            var allIdentities = GameObject.FindObjectsByType<PlayerIdentity>(FindObjectsSortMode.None);
            int orphansFound = 0;
            
            foreach (var identity in allIdentities)
            {
                if (identity.userId == userId)
                {
                    orphansFound++;
                    Debug.Log($"[PlayerManager] Ghost/Huérfano #{orphansFound} eliminado para UID: {userId}");
                    Destroy(identity.gameObject);
                }
            }
            
            if (orphansFound > 0)
            {
                Debug.Log($"[PlayerManager] Objetos extra eliminados: {orphansFound}");
            }
        }

        public List<string> GetAllCharacterIds()
        {
            var ids = new HashSet<string>();
            foreach (var m in lobbyCharacterPrefabs) if (!string.IsNullOrEmpty(m.characterId)) ids.Add(m.characterId);
            foreach (var m in gameCharacterPrefabs) if (!string.IsNullOrEmpty(m.characterId)) ids.Add(m.characterId);
            return new List<string>(ids);
        }

        public string GetPlayerName(string userId)
        {
            string resolvedName = "Jugador";
            if (_playerNames.TryGetValue(userId, out string name))
            {
                resolvedName = name;
            }

            return resolvedName;
        }

        public int GetPlayerLevel(string userId)
        {
            int resolvedLevel = 1;
            if (_playerLevels.TryGetValue(userId, out int level))
            {
                resolvedLevel = level;
            }

            return resolvedLevel;
        }

        public void CleanupAllBots()
        {
            Debug.Log("[PlayerManager] Limpiando todos los bots...");
            
            // Eliminar todos los bots de las estructuras
            var botIds = _connectionOrder.Where(id => id.StartsWith("BOT_")).ToList();
            foreach (var botId in botIds)
            {
                Debug.Log($"[PlayerManager] Eliminando bot: {botId}");
                
                // Destruir objeto físico si existe
                if (_playerObjects.TryGetValue(botId, out GameObject botObj))
                {
                    if (botObj != null) Destroy(botObj);
                    _playerObjects.Remove(botId);
                }
                
                // Limpiar datos
                _playerNames.Remove(botId);
                _playerLevels.Remove(botId);
                _playerLastCharId.Remove(botId);
                _connectionOrder.Remove(botId);
            }
            
            Debug.Log($"[PlayerManager] Cleanup completado. {botIds.Count} bots eliminados.");
        }

        
        public interface IChibitsController
        {
            void ProcessJoystick(float x, float y);
            void ProcessButton(string buttonId, string state);
        }
    }
}
