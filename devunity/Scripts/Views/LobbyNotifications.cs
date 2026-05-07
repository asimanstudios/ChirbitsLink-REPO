using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace ChibitsLink.UI
{
    /// <summary>
    /// Gestiona notificaciones en pantalla para conexiones/desconexiones de jugadores.
    /// Muestra mensajes temporales en la UI del lobby.
    /// </summary>
    /// <remarks>
    /// Utiliza una cola para procesar múltiples notificaciones.
    /// Evita superposición de mensajes con procesamiento secuencial.
    /// Integrado con TextMeshPro para renderizado de texto.
    /// </remarks>
    public class LobbyNotifications : MonoBehaviour
    {
        /// <summary>Componente de texto para mostrar notificaciones</summary>
        public TextMeshProUGUI notificationText;
        /// <summary>Tiempo de visualización de cada notificación</summary>
        public float displayTime = 3f;
        
        /// <summary>Cola de mensajes pendientes de mostrar</summary>
        private Queue<string> _messageQueue = new Queue<string>();
        /// <summary>Indica si se está mostrando una notificación</summary>
        private bool _isShowing = false;

        /// <summary>
        /// Inicialización del componente.
        /// Limpia el texto de notificaciones al inicio.
        /// </summary>
        private void Awake()
        {
            if (notificationText != null)
                notificationText.text = "";
        }

        /// <summary>
        /// Muestra una notificación en pantalla.
        /// Añade el mensaje a la cola y procesa si no hay activa.
        /// </summary>
        /// <param name="message">Mensaje a mostrar</param>
        public void ShowNotification(string message)
        {
            _messageQueue.Enqueue(message);
            if (!_isShowing)
            {
                StartCoroutine(ProcessQueue());
            }
        }

        /// <summary>
        /// Procesa la cola de notificaciones de forma secuencial.
        /// Muestra cada mensaje por el tiempo configurado.
        /// </summary>
        /// <returns>Coroutine para procesamiento asíncrono</returns>
        private IEnumerator ProcessQueue()
        {
            _isShowing = true;
            while (_messageQueue.Count > 0)
            {
                string msg = _messageQueue.Dequeue();
                if (notificationText != null)
                {
                    notificationText.text = msg;
                    yield return new WaitForSeconds(displayTime);
                    notificationText.text = "";
                }
                yield return new WaitForSeconds(0.5f);
            }
            _isShowing = false;
        }
    }
}
