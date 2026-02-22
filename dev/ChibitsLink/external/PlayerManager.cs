using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Gestiona los jugadores conectados al servidor, asignándoles un ID de mando (P1, P2...)
    /// y gestionando sus prefabs en tiempo real.
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [Header("Configuración de Spawn")]
        public List<Transform> spawnPoints; // Lista de transforms para posicionar P1, P2, etc.
        
        [Header("Prefabs de Personajes")]
        public List<CharacterPrefabMap> characterPrefabs; // Mapeo de ID a Prefab
        
        [Header("Límites")]
        public int maxPlayers = 4; // Máximo 4 jugadores

        [Serializable]
        public struct CharacterPrefabMap
        {
            public string characterId;
            public GameObject prefab;
        }

        private Dictionary<string, GameObject> _playerObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, string> _playerCharacters = new Dictionary<string, string>(); // userId -> charId
        private List<string> _connectionOrder = new List<string>(); // Para P1, P2...

        /// <summary>
        /// Registra un nuevo jugador o actualiza su estado.
        /// </summary>
        public bool HandlePlayerJoin(string userId, string initialCharId)
        {
            // Verificar límite de jugadores
            if (_connectionOrder.Count >= maxPlayers && !_connectionOrder.Contains(userId))
            {
                Debug.LogWarning($"[PlayerManager] No se puede agregar más jugadores. Límite de {maxPlayers} alcanzado.");
                return false;
            }
            
            if (!_connectionOrder.Contains(userId))
            {
                _connectionOrder.Add(userId);
            }

            int playerNumber = _connectionOrder.IndexOf(userId);
            
            // Guardar el personaje seleccionado
            _playerCharacters[userId] = initialCharId;
            
            SpawnOrUpdatePlayer(userId, initialCharId, playerNumber);
            
            Debug.Log($"[PlayerManager] Jugador {userId} unido como Player {playerNumber + 1} con personaje {initialCharId}");
            return true;
        }

        /// <summary>
        /// Sincroniza el personaje del jugador (cuando cambia desde la app).
        /// </summary>
        public void HandleCharacterSync(string userId, string newCharId)
        {
            if (_playerObjects.ContainsKey(userId))
            {
                int playerNumber = _connectionOrder.IndexOf(userId);
                
                // Actualizar el personaje guardado
                _playerCharacters[userId] = newCharId;
                
                SpawnOrUpdatePlayer(userId, newCharId, playerNumber);
                
                Debug.Log($"[PlayerManager] Personaje de {userId} cambiado a {newCharId}");
            }
            else
            {
                Debug.LogWarning($"[PlayerManager] No se puede cambiar personaje: usuario {userId} no encontrado");
            }
        }

        private void SpawnOrUpdatePlayer(string userId, string charId, int playerIndex)
        {
            // Eliminar modelo anterior si existe
            if (_playerObjects.TryGetValue(userId, out GameObject oldPlayer))
            {
                Destroy(oldPlayer);
            }

            // Buscar el prefab correspondiente
            GameObject prefabToSpawn = null;
            foreach (var map in characterPrefabs)
            {
                if (map.characterId == charId)
                {
                    prefabToSpawn = map.prefab;
                    break;
                }
            }
            
            if (prefabToSpawn == null && characterPrefabs.Count > 0) 
            {
                prefabToSpawn = characterPrefabs[0].prefab; // Default
                Debug.LogWarning($"[PlayerManager] Personaje {charId} no encontrado, usando default");
            }

            if (prefabToSpawn != null)
            {
                Transform spawnPoint = (playerIndex < spawnPoints.Count) ? spawnPoints[playerIndex] : transform;
                GameObject newPlayer = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
                
                // Configurar etiquetas o IDs en el objeto para el sistema de control
                newPlayer.name = $"Player_{playerIndex + 1}_{userId}";
                newPlayer.tag = $"Player{playerIndex + 1}"; // Para identificar P1, P2, P3, P4
                
                _playerObjects[userId] = newPlayer;
                
                Debug.Log($"[PlayerManager] Player {playerIndex + 1} ({userId}) spawneado con {charId}");
            }
            else
            {
                Debug.LogError($"[PlayerManager] No se pudo spawnear jugador {userId}: ningún prefab disponible");
            }
        }

        /// <summary>
        /// Procesa los inputs del mando desde la app.
        /// </summary>
        public void HandleControllerInput(string userId, string inputData)
        {
            if (_playerObjects.TryGetValue(userId, out GameObject playerObj))
            {
                // Aquí se procesaría el input JSON
                // Ejemplo: playerObj.GetComponent<PlayerController>().ProcessInput(inputData);
                // Por ahora solo logueamos
                Debug.Log($"[PlayerManager] Input recibido de {userId}: {inputData}");
            }
        }

        /// <summary>
        /// Maneja la desconexión de un jugador.
        /// </summary>
        public void HandlePlayerDisconnect(string userId)
        {
            if (_playerObjects.TryGetValue(userId, out GameObject playerObj))
            {
                Destroy(playerObj);
                _playerObjects.Remove(userId);
            }
            
            _playerCharacters.Remove(userId);
            _connectionOrder.Remove(userId);
            
            Debug.Log($"[PlayerManager] Jugador {userId} desconectado y eliminado.");
            
            // Reorganizar los números de jugador restantes
            RebuildPlayerNumbers();
        }
        
        /// <summary>
        /// Reorganiza los números de jugador después de una desconexión.
        /// </summary>
        private void RebuildPlayerNumbers()
        {
            int index = 0;
            foreach (var userId in _connectionOrder.ToList())
            {
                if (_playerObjects.TryGetValue(userId, out GameObject playerObj))
                {
                    playerObj.name = $"Player_{index + 1}_{userId}";
                    playerObj.tag = $"Player{index + 1}";
                    
                    // Mover al spawn point correspondiente
                    if (index < spawnPoints.Count)
                    {
                        playerObj.transform.position = spawnPoints[index].position;
                        playerObj.transform.rotation = spawnPoints[index].rotation;
                    }
                }
                index++;
            }
        }
        
        /// <summary>
        /// Obtiene el número actual de jugadores conectados.
        /// </summary>
        public int GetPlayerCount()
        {
            return _connectionOrder.Count;
        }
        
        /// <summary>
        /// Verifica si la sala está llena.
        /// </summary>
        public bool IsRoomFull()
        {
            return _connectionOrder.Count >= maxPlayers;
        }
        
        /// <summary>
        /// Obtiene información de un jugador específico.
        /// </summary>
        public (int playerNumber, string characterId)? GetPlayerInfo(string userId)
        {
            if (_connectionOrder.Contains(userId))
            {
                int playerNumber = _connectionOrder.IndexOf(userId);
                string charId = _playerCharacters.ContainsKey(userId) ? _playerCharacters[userId] : "DEFAULT";
                return (playerNumber, charId);
            }
            return null;
        }
    }
}
