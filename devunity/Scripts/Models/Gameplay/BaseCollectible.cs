using UnityEngine;
using Chirbits.Core;

namespace ChibitsLink.GameSide.Models
{
    /// <summary>
    /// Clase base abstracta para cualquier minijuego de recolección de objetos.
    /// Preserva la compatibilidad del motor de Serialización de Unity usando los
    /// mismos nombres de variables para no corromper los prefabs de Moneda.
    /// </summary>
    /// <remarks>
    /// Proporciona funcionalidad común para todos los objetos recolectables.
    /// Maneja rotación visual, efectos y sonido de recolección.
    /// Las subclases deben implementar la lógica específica de recolección.
    /// </remarks>
    public abstract class BaseCollectible : MonoBehaviour
    {
        [Header("Efectos")]
        /// <summary>Efecto visual al recolectar</summary>
        public GameObject efectoColeccion;
        /// <summary>Tiempo de vida del efecto</summary>
        public float tiempoVidaEfecto = 2f;
        /// <summary>Sonido al recolectar</summary>
        public AudioClip sonidoColeccion;
        /// <summary>Velocidad de rotación visual</summary>
        public float rotacionVelocidad = 100f;

        [Header("Puntuación")]
        /// <summary>Valor en puntos del objeto</summary>
        public int valor = 1;

        /// <summary>
        /// Actualización del objeto cada frame.
        /// Maneja la rotación visual generalizada.
        /// </summary>
        protected virtual void Update()
        {
            // Rotación visual generalizada
            if (rotacionVelocidad > 0)
            {
                transform.Rotate(Vector3.up, rotacionVelocidad * Time.deltaTime);
            }
        }

        /// <summary>
        /// Maneja la colisión con otros objetos.
        /// Procesa la recolección cuando un jugador entra en contacto.
        /// </summary>
        /// <param name="other">Collider del objeto que colisionó</param>
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

        /// <summary>
        /// Dispara los efectos visuales y de sonido de recolección.
        /// Crea efectos temporales y reproduce sonido en la posición.
        /// </summary>
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
        /// Comprueba si el estado actual de la partida permite recoger el objeto.
        /// Depende de las reglas específicas de cada minijuego.
        /// </summary>
        /// <returns>True si el objeto puede ser recolectado</returns>
        protected abstract bool CanBeCollected();

        /// <summary>
        /// Sincroniza la recolección con el gestor del minijuego.
        /// Notifica al gestor correspondiente sobre la recolección.
        /// </summary>
        /// <param name="userId">ID del usuario que recolectó</param>
        protected abstract void OnCollect(string userId);
    }
}
