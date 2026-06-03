using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Gestor principal de jugadores con persistencia entre escenas.
    /// Maneja spawn, personajes, niveles y conexión de jugadores.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    /// <remarks>
    /// Versión final con 2 listas (Lobby y Juego).
    /// Persiste datos entre escenas y gestiona spawn ordenado.
    /// Soporta bots y limpieza automática.
    /// </remarks>
    public class PlayerManager : MonoBehaviour
    {
        /// <summary>Instancia global del PlayerManager (patrón Singleton)</summary>
        public static PlayerManager Instance { get; private set; }

        [Header("Configuración de Spawn")]
        /// <summary>Puntos de spawn para jugadores</summary>
        public List<Transform> spawnPoints; 
        /// <summary>Sistema de notificaciones del lobby</summary>
        public ChibitsLink.UI.LobbyNotifications notifications; 
        
        [Header("Prefabs de Personajes")]
        /// <summary>Prefabs de personajes para el lobby</summary>
        public List<CharacterPrefabMap> lobbyCharacterPrefabs; 
        /// <summary>Prefabs de personajes para el juego</summary>
        [FormerlySerializedAs("characterPrefabs")]
        public List<CharacterPrefabMap> gameCharacterPrefabs;

        /// <summary>
        /// Mapeo de ID de personaje a prefab.
        /// Define posición, rotación y escala para cada personaje.
        /// </summary>
        [Serializable]
        public struct CharacterPrefabMap
        {
            /// <summary>ID del personaje</summary>
            public string characterId;
            /// <summary>Prefab del personaje</summary>
            public GameObject prefab;
            /// <summary>Offset de posición</summary>
            public Vector3 positionOffset; 
            /// <summary>Offset de rotación</summary>
            public Vector3 rotationOffset; 
            /// <summary>Escala local</summary>
            public Vector3 localScale;     
        }

        /// <summary>Objetos de jugadores instanciados</summary>
        private Dictionary<string, GameObject> _playerObjects = new Dictionary<string, GameObject>();
        /// <summary>Nombres de jugadores conectados</summary>
        private Dictionary<string, string> _playerNames = new Dictionary<string, string>();
        /// <summary>Niveles de jugadores</summary>
        private Dictionary<string, int> _playerLevels = new Dictionary<string, int>();
        /// <summary>Último personaje seleccionado por jugador</summary>
        private Dictionary<string, string> _playerLastCharId = new Dictionary<string, string>();
        /// <summary>Orden de conexión de jugadores</summary>
        private List<string> _connectionOrder = new List<string>(); 
        /// <summary>Indica si hay transición de escena en curso</summary>
        private bool isTransitioning = false;

        /// <summary>
        /// Inicializa el PlayerManager y establece el patrón Singleton.
        /// Configura persistencia entre escenas y spawn points.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(transform.root.gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
                FindSpawnPointsInScene();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Limpia recursos al destruir el objeto.
        /// Remueve listeners y limpia bots.
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this) 
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                CleanupAllBots(); // Limpiar todos los bots al salir
            }
        }

        /// <summary>
        /// Maneja el evento de carga de escena.
        /// Prepara spawn ordenado de jugadores.
        /// </summary>
        /// <param name="scene">Escena cargada</param>
        /// <param name="mode">Modo de carga</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[PlayerManager] Escena '{scene.name}' cargada. Preparando spawn ordenado...");
            
            isTransitioning = true;
            _playerObjects.Clear();

            StartCoroutine(DelayedSpawnRoutine());
        }

        /// <summary>
        /// Rutina de spawn con retraso.
        /// Limpia residuos y spawnea jugadores ordenadamente.
        /// </summary>
        /// <returns>IEnumerator para la corrutina</returns>
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
            string charId;
            int spawnIndex;
            bool hasCharId;
            foreach (var userId in _connectionOrder)
            {
                hasCharId = _playerLastCharId.TryGetValue(userId, out charId);
                if (hasCharId)
                {
                    // Usar módulo para evitar solapamiento si hay más jugadores que spawn points
                    spawnIndex = idx % spawnPoints.Count;
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
            }
            else
            {
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
            bool isPlayerCam;
            GameObject playerObjForCam;
            bool enableCam;
            Camera playerCam;
            AudioListener listener;
            bool hasTargetTexture;
            bool shouldProcessCamera;
            bool shouldSetSceneCamera;

            foreach (var cam in allCams)
            {
                hasTargetTexture = cam.targetTexture != null;
                shouldProcessCamera = !hasTargetTexture;
                if (!shouldProcessCamera)
                {
                    // ignorar render textures (minimapas, etc.)
                }
                else
                {

                // Comprobar si pertenece a algún jugador instanciado
                isPlayerCam = false;
                bool isChildOfPlayer;
                foreach (var playerObj in _playerObjects.Values)
                {
                    playerObjForCam = playerObj;
                    isChildOfPlayer = playerObjForCam != null && cam.transform.IsChildOf(playerObjForCam.transform);
                    if (isChildOfPlayer)
                    {
                        isPlayerCam = true;
                    }
                }

                    shouldSetSceneCamera = !isPlayerCam && sceneCamera == null;
                    if (shouldSetSceneCamera)
                    {
                        sceneCamera = cam;
                    }
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
            GameObject currentObj;
            bool hasValidPlayer;
            foreach (var userId in _connectionOrder)
            {
                hasValidPlayer = _playerObjects.TryGetValue(userId, out currentObj) && currentObj != null;
                if (hasValidPlayer)
                {
                    // Solo P1 puede tener cámara cuando NO hay cámara de escena
                    enableCam = !hasSceneCamera && idx == 0;

                    playerCam = currentObj.GetComponentInChildren<Camera>(true);
                    if (playerCam != null) playerCam.gameObject.SetActive(enableCam);

                    listener = currentObj.GetComponentInChildren<AudioListener>(true);
                    if (listener != null) listener.enabled = enableCam;
                }

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
            }
            else
            {
                ProcessInputType(controller, input);
            }
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
            GameObject botObj;
            bool hasBotObject;
            foreach (var botId in botIds)
            {
                Debug.Log($"[PlayerManager] Eliminando bot: {botId}");
                
                // Destruir objeto físico si existe
                hasBotObject = _playerObjects.TryGetValue(botId, out botObj);
                if (hasBotObject)
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
