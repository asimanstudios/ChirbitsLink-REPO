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

        [Serializable]
        public struct CharacterPrefabMap
        {
            public string characterId;
            public GameObject prefab;
        }

        private Dictionary<string, GameObject> _playerObjects = new Dictionary<string, GameObject>();
        private List<string> _connectionOrder = new List<string>(); // Para P1, P2...

        /// <summary>
        /// Registra un nuevo jugador o actualiza su estado.
        /// </summary>
        public void HandlePlayerJoin(string userId, string initialCharId)
        {
            if (!_connectionOrder.Contains(userId))
            {
                _connectionOrder.Add(userId);
            }

            int playerNumber = _connectionOrder.IndexOf(userId);
            SpawnOrUpdatePlayer(userId, initialCharId, playerNumber);
        }

        public void HandleCharacterSync(string userId, string newCharId)
        {
            if (_playerObjects.ContainsKey(userId))
            {
                int playerNumber = _connectionOrder.IndexOf(userId);
                SpawnOrUpdatePlayer(userId, newCharId, playerNumber);
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
            GameObject prefabToSpawn = characterPrefabs.Find(m => m.characterId == charId).prefab;
            if (prefabToSpawn == null && characterPrefabs.Count > 0) 
            {
                prefabToSpawn = characterPrefabs[0].prefab; // Default
            }

            if (prefabToSpawn != null)
            {
                Transform spawnPoint = (playerIndex < spawnPoints.Count) ? spawnPoints[playerIndex] : transform;
                GameObject newPlayer = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
                
                // Configurar etiquetas o IDs en el objeto para el sistema de control
                newPlayer.name = $"Player_{playerIndex + 1}_{userId}";
                
                _playerObjects[userId] = newPlayer;
                
                Debug.Log($"[PlayerManager] Player {playerIndex + 1} ({userId}) spawneado con {charId}");
            }
        }

        public void HandleControllerInput(string userId, string inputData)
        {
            // Aquí se redirigiría el input al objeto correspondiente en _playerObjects[userId]
            if (_playerObjects.TryGetValue(userId, out GameObject playerObj))
            {
                // Ejemplo: playerObj.GetComponent<PlayerController>().ProcessInput(inputData);
            }
        }

        public void HandlePlayerDisconnect(string userId)
        {
            if (_playerObjects.TryGetValue(userId, out GameObject playerObj))
            {
                Destroy(playerObj);
                _playerObjects.Remove(userId);
                _connectionOrder.Remove(userId);
                Debug.Log($"[PlayerManager] Jugador {userId} desconectado y eliminado.");
            }
        }
    }
}
