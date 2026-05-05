using UnityEngine;
using ChibitsLink.GameSide.Models;

namespace ChibitsLink.GameSide.HookParty
{
    /// <summary>
    /// Hereda de BaseCollectible para compartir lógica (VFX, rotación) con otros items 
    /// como las Monedas del minijuego CoinCollector.
    /// </summary>
    public class CollectibleItem : BaseCollectible
    {
        protected override bool CanBeCollected()
        {
            // Solo se puede recoger si el juego HookParty está activo
            // Si el manager no existe, permitimos recoger para facilitar tests en escenas aisladas
            bool canCollect = true;
            if (HookPartyManager.Instance != null)
            {
                canCollect = HookPartyManager.Instance.IsPlaying;
            }

            return canCollect;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (CanBeCollected())
            {
                // Buscamos cualquier señal de que es el jugador:
                // 1. Etiqueta "Player" en el objeto o en el root
                // 2. Componente PlayerIdentity o HookPartyController en la jerarquía
                bool isPlayer = other.CompareTag("Player") || (other.transform.root != null && other.transform.root.CompareTag("Player"));
                
                PlayerIdentity identity = other.GetComponentInParent<PlayerIdentity>();
                HookPartyController controller = other.GetComponentInParent<HookPartyController>();

                if (isPlayer || identity != null || controller != null)
                {
                    // Intentamos obtener el userId para los puntos
                    string uid = "";
                    if (identity != null) uid = identity.userId;
                    if (identity == null && controller != null)
                    {
                        var idComp = controller.GetComponent<PlayerIdentity>();
                        if (idComp != null) uid = idComp.userId;
                    }

                    // Ejecutamos la recogida (incluso si uid es vacío, para que al menos desaparezca el item)
                    OnCollect(uid);
                    TriggerVisualEffects();
                    Destroy(gameObject);
                }
            }
        }

        protected override void OnCollect(string userId)
        {
            // Añade los puntos heredados ('valor') al jugador
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(userId, valor);
            }
            else
            {
                Debug.LogWarning("[HookParty] ScoreManager no encontrado.");
            }
        }
    }
}
