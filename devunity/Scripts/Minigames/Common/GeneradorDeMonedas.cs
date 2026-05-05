using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ChibiCocina.CoinCollector
{
    [System.Serializable]
    public class CoinTier
    {
        public string nombre;
        public GameObject prefab;
        public float probabilidad = 1.0f;
    }

    public class GeneradorDeMonedas : MonoBehaviour
    {
        public static GeneradorDeMonedas Instancia { get; private set; }

        [Header("Configuración de Spawn")]
        public List<CoinTier> tiersMonedas;
        public BoxCollider areaDeSpawn;
        public int maxMonedas = 20;
        public float intervaloSpawn = 2.0f;
        public LayerMask capasObstaculos;

        [Header("Audio")]
        public AudioClip sonidoSpawn;

        private List<GameObject> monedasActivas = new List<GameObject>();
        private float timer = 0f;

        private void Awake()
        {
            if (Instancia == null) Instancia = this;
            else Destroy(this);
        }

        private void Update()
        {
            // Solo spawnear si el juego está activo
            bool isInGame = GestorCoinCollector.Instancia != null && GestorCoinCollector.Instancia.estadoActual == GameState.InGame;
            if (isInGame)
            {
                timer += Time.deltaTime;
                if (timer >= intervaloSpawn)
                {
                    timer = 0f;
                    LimpiarListaMonedas();

                    if (monedasActivas.Count < maxMonedas)
                    {
                        IntentarSpawnear();
                    }
                }
            }
        }

        private void LimpiarListaMonedas()
        {
            // Eliminar referencias nulas (monedas recogidas)
            monedasActivas.RemoveAll(m => m == null);
        }

        private void IntentarSpawnear()
        {
            bool hasCoinTiers = tiersMonedas != null && tiersMonedas.Count > 0;
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
                        if (!Physics.CheckSphere(spawnPos, 0.5f, capasObstaculos))
                        {
                            posicionValida = true;
                        }
                    }

                    if (posicionValida)
                    {
                        GameObject moneda = Instantiate(tierSeleccionado.prefab, spawnPos, Quaternion.identity);
                        monedasActivas.Add(moneda);

                        if (sonidoSpawn != null)
                        {
                            AudioSource.PlayClipAtPoint(sonidoSpawn, spawnPos, 0.8f);
                        }
                    }
                }
            }
        }

        private CoinTier SeleccionarTierAleatorio()
        {
            float sumaPesos = 0;
            foreach (var t in tiersMonedas) sumaPesos += t.probabilidad;

            float randomValue = Random.Range(0, sumaPesos);
            float acumulado = 0;

            foreach (var t in tiersMonedas)
            {
                acumulado += t.probabilidad;
                if (randomValue <= acumulado) return t;
            }

            return tiersMonedas[0];
        }

        private Vector3 ObtenerPuntoAleatorioArea()
        {
            if (areaDeSpawn == null) return transform.position;

            Bounds bounds = areaDeSpawn.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                transform.position.y, // Mantener altura del generador o suelo
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        // Visualización del área en el editor
        private void OnDrawGizmosSelected()
        {
            if (areaDeSpawn != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawCube(areaDeSpawn.bounds.center, areaDeSpawn.bounds.size);
            }
        }
    }
}
