using UnityEngine;
using ChibitsLink.GameSide.Models;

namespace ChibitsLink.GameSide.HookParty
{
    /// <summary>
    /// Controla la aparición y destrucción instanciada de los 
    /// coleccionables dentro del área de la caja del minijuego.
    /// </summary>
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [Tooltip("Prefabs a spawnear de manera aleatoria")]
        public GameObject[] itemPrefabs;
        [Tooltip("Cada cuantos segundos aparecerá un item en el mapa")]
        public float spawnInterval = 3f;
        
        [Header("Area Configuration")]
        [Tooltip("Asigna un BoxCollider (Trigger) para definir el volumen de aparición")]
        public BoxCollider spawnVolume;
        
        [Tooltip("Límite máximo de items en pantalla")]
        public int maxItemsOnScreen = 15;

        [Tooltip("Multiplicador de tamaño para los items spawneados")]
        public float itemScaleMultiplier = 2.2f;

        private float _timer;

        private void Update()
        {
            // Prevenir spawneo si la partida no está en curso
            bool canSpawnByState = HookPartyManager.Instance == null || HookPartyManager.Instance.IsPlaying;
            bool hasSpawnCapacity = transform.childCount < maxItemsOnScreen;
            if (canSpawnByState && hasSpawnCapacity)
            {
                _timer += Time.deltaTime;
                if (_timer >= spawnInterval)
                {
                    _timer = 0f;
                    SpawnRandomItem();
                }
            }
        }

        private void SpawnRandomItem()
        {
            bool canSpawnItem = itemPrefabs != null && itemPrefabs.Length > 0 && spawnVolume != null;
            if (canSpawnItem)
            {
                // Calcular punto aleatorio dentro de los límites del BoxCollider
                Bounds bounds = spawnVolume.bounds;
                float rx = Random.Range(bounds.min.x, bounds.max.x);
                float ry = Random.Range(bounds.min.y, bounds.max.y);
                
                // Mantenemos la Z original del centro del collider o 0 para el plano 2.5D
                float rz = spawnVolume.transform.position.z;

                Vector3 targetPosition = new Vector3(rx, ry, rz);

                GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
                GameObject item = Instantiate(prefab, targetPosition, Quaternion.identity, transform);
                
                // Aplicar escala aumentada
                item.transform.localScale *= itemScaleMultiplier;

                // COMPATIBILIDAD: Si el prefab viene de otro minijuego (ej: CoinCollector)
                // tendrá el script 'Moneda'. Lo cambiamos por 'CollectibleItem' para que sea recolectable aquí.
                BaseCollectible oldComp = item.GetComponent<BaseCollectible>();
                if (oldComp != null && !(oldComp is CollectibleItem))
                {
                    int savedValue = oldComp.valor;
                    GameObject vfx = oldComp.efectoColeccion;
                    AudioClip sfx = oldComp.sonidoColeccion;

                    Destroy(oldComp);
                    
                    CollectibleItem newComp = item.AddComponent<CollectibleItem>();
                    newComp.valor = savedValue;
                    newComp.efectoColeccion = vfx;
                    newComp.sonidoColeccion = sfx;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (spawnVolume != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                Gizmos.DrawCube(spawnVolume.bounds.center, spawnVolume.bounds.size);
            }
        }
    }
}
