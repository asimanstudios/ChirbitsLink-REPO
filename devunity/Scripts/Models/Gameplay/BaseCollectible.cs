using UnityEngine;
using Chirbits.Core;

namespace ChibitsLink.GameSide.Models
{
    /// <summary>
    /// Clase base abstracta para cualquier minijuego de recolección de objetos.
    /// Preserva la compatibilidad del motor de Serialización de Unity usando los
    /// mismos nombres de variables para no corromper los prefabs de Moneda.
    /// </summary>
    public abstract class BaseCollectible : MonoBehaviour
    {
        [Header("Efectos")]
        public GameObject efectoColeccion;
        public float tiempoVidaEfecto = 2f;
        public AudioClip sonidoColeccion;
        public float rotacionVelocidad = 100f;

        [Header("Puntuación")]
        public int valor = 1;

        protected virtual void Update()
        {
            // Rotación visual generalizada
            if (rotacionVelocidad > 0)
            {
                transform.Rotate(Vector3.up, rotacionVelocidad * Time.deltaTime);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // Si el GameManager actual no deja coger o ya se cogió, no seguimos
            if (CanBeCollected() && other.CompareTag("Player"))
            {
                // Optimización Crítica: Usar el Gestor central para obtener la identidad de la caché
                // Esto evita el freeze del Editor al colisionar con muchos objetos.
                var manager = GameObject.FindObjectOfType<BaseMinigameManager>();
                if (manager != null)
                {
                    var identity = manager.GetIdentity(other.gameObject);
                    if (identity != null && !string.IsNullOrEmpty(identity.userId))
                    {
                        OnCollect(identity.userId);
                        TriggerVisualEffects();
                        Destroy(gameObject);
                    }
                }
            }
        }

        protected virtual void TriggerVisualEffects()
        {
            if (efectoColeccion != null)
            {
                GameObject vfx = Instantiate(efectoColeccion, transform.position, Quaternion.identity);
                Destroy(vfx, tiempoVidaEfecto);
            }

            if (sonidoColeccion != null)
            {
                AudioSource.PlayClipAtPoint(sonidoColeccion, transform.position, 1f);
            }
        }

        /// <summary>
        /// Comprueba si el estado actual de la partida (dependiendo del minijuego) permite recoger el item.
        /// </summary>
        protected abstract bool CanBeCollected();

        /// <summary>
        /// Sincroniza la recogida con el Manager de su minijuego particular.
        /// </summary>
        protected abstract void OnCollect(string userId);
    }
}
