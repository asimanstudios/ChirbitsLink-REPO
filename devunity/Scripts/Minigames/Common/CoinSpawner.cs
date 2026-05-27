using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ChibitsLink.Minigames.Common
{
    [System.Serializable]
    public class CoinTier
    {
        public string name;
        public GameObject prefab;
        public float probability = 1.0f;
    }

    public class CoinSpawner : MonoBehaviour
    {
        public static CoinSpawner Instance { get; private set; }

        [Header("Spawn Configuration")]
        public List<CoinTier> coinTiers;
        public BoxCollider spawnArea;
        public int maxCoins = 20;
        public float spawnInterval = 2.0f;
        public LayerMask obstacleLayers;

        [Header("Audio")]
        public AudioClip spawnSound;

        private List<GameObject> _activeCoins = new List<GameObject>();
        private float _timer = 0f;

        private void Awake()
        {
            if (Instance == null) 
            {
                Instance = this;
            }
            else 
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            // Only spawn if game is active
            bool isInGame = CoinCollectorGameManager.Instance != null && 
                            CoinCollectorGameManager.Instance.CurrentState == CoinCollectorState.InGame;
            
            if (isInGame)
            {
                _timer += Time.deltaTime;
                if (_timer >= spawnInterval)
                {
                    _timer = 0f;
                    CleanupCoinList();

                    if (_activeCoins.Count < maxCoins)
                    {
                        SpawnCoin();
                    }
                }
            }
        }

        private void CleanupCoinList()
        {
            // Eliminar referencias nulas (monedas recogidas)
            _activeCoins.RemoveAll(m => m == null);
        }

        private void SpawnCoin()
        {
            bool hasCoinTiers = coinTiers != null && coinTiers.Count > 0;
            if (hasCoinTiers)
            {
                // 1. Seleccionar Tier mediante Random con Pesos
                CoinTier tierSeleccionado = SeleccionarTierAleatorio();
                bool canSpawnTier = tierSeleccionado != null && tierSeleccionado.prefab != null;
                if (canSpawnTier)
                {
                    // 2. Intentar encontrar una posición válida (sin obstáculos)
                    Vector3 spawnPos = Vector3.zero;
                    bool posicionValida = false;
                    int intentos = 0;

                    while (!posicionValida && intentos < 30)
                    {
                        intentos++;
                        spawnPos = ObtenerPuntoAleatorioArea();

                        // Chequeo de colisión (Esfera invisible)
                        // Se asume que las monedas tienen un radio pequeño (~0.5f)
                        if (!Physics.CheckSphere(spawnPos, 0.5f, obstacleLayers))
                        {
                            posicionValida = true;
                        }
                    }

                    if (posicionValida)
                    {
                        GameObject moneda = Instantiate(tierSeleccionado.prefab, spawnPos, Quaternion.identity);
                        _activeCoins.Add(moneda);

                        if (spawnSound != null)
                        {
                            AudioSource.PlayClipAtPoint(spawnSound, spawnPos, 0.8f);
                        }
                    }
                }
            }
        }

        private CoinTier SeleccionarTierAleatorio()
        {
            float sumaPesos = 0;
            foreach (var t in coinTiers) sumaPesos += t.probability;

            float randomValue = Random.Range(0, sumaPesos);
            float acumulado = 0;
            CoinTier selectedTier = null;

            foreach (var t in coinTiers)
            {
                if (selectedTier == null)
                {
                    acumulado += t.probability;
                    if (randomValue <= acumulado)
                    {
                        selectedTier = t;
                    }
                }
            }

            if (selectedTier == null)
            {
                selectedTier = coinTiers[0];
            }

            return selectedTier;
        }

        private Vector3 ObtenerPuntoAleatorioArea()
        {
            Vector3 result = transform.position;
            if (spawnArea != null)
            {
                Bounds bounds = spawnArea.bounds;
                result = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    transform.position.y, // Mantener altura del generador o suelo
                    Random.Range(bounds.min.z, bounds.max.z)
                );
            }
            return result;
        }

        // Visualización del área en el editor
        private void OnDrawGizmosSelected()
        {
            if (spawnArea != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawCube(spawnArea.bounds.center, spawnArea.bounds.size);
            }
        }
    }
}
