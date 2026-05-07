using UnityEngine;
using System.Collections.Generic;

namespace ChibitsLink.Views
{
    /// <summary>
    /// Controlador de cámara para seguimiento dinámico de múltiples objetivos.
    /// Mantiene todos los objetivos en frame con zoom automático.
    /// Suaviza movimiento y ajuste de zoom para mejor experiencia visual.
    /// </summary>
    /// <remarks>
    /// Ideal para juegos multijugador local o escenas con múltiples actores.
    /// Calcula automáticamente el punto central y distancia óptima.
    /// </remarks>
    public class CameraController : MonoBehaviour
    {
        /// <summary>
        /// Configuración de objetivos a seguir.
        /// </summary>
        [Header("Target Configuration")]
        /// <summary>Lista de objetivos que la cámara debe seguir</summary>
        public List<Transform> targets;

        /// <summary>
        /// Configuración de offset de cámara.
        /// </summary>
        [Header("Camera Offset")]
        /// <summary>Offset de posición relativo al centro de los objetivos</summary>
        public Vector3 offset = new Vector3(0, 10, -10);

        /// <summary>
        /// Configuración de suavizado de movimiento.
        /// </summary>
        [Header("Smoothing")]
        /// <summary>Tiempo de suavizado para movimiento de cámara</summary>
        public float smoothTime = 0.5f;
        private Vector3 _velocity;

        /// <summary>
        /// Configuración de zoom dinámico.
        /// </summary>
        [Header("Dynamic Zoom")]
        /// <summary>Zoom mínimo cuando los objetivos están cerca</summary>
        public float minZoom = 8f;
        /// <summary>Zoom máximo cuando los objetivos están lejos</summary>
        public float maxZoom = 20f;
        /// <summary>Distancia límite para ajuste de zoom</summary>
        public float zoomLimiter = 50f;
        /// <summary>Tiempo de suavizado para ajuste de zoom</summary>
        public float zoomSmoothTime = 0.5f;

        private Camera _camera;
        private float _zoomVelocity;

        /// <summary>
        /// Inicializa el componente Camera.
        /// Obtiene la referencia al componente Camera del GameObject.
        /// </summary>
        private void Start()
        {
            _camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Actualiza la posición y zoom de la cámara cada frame.
        /// Se ejecuta en LateUpdate para asegurar que todos los objetivos se movieron primero.
        /// </summary>
        /// <remarks>
        /// LateUpdate se ejecuta después de todos los Update y FixedUpdate.
        /// Solo actualiza si hay objetivos configurados.
        /// </remarks>
        private void LateUpdate()
        {
            bool hasTargets = targets.Count > 0;
            if (hasTargets)
            {
                MoveCamera();
                UpdateZoom();
            }
        }

        /// <summary>
        /// Mueve la cámara hacia el punto central de todos los objetivos.
        /// Aplica suavizado y mantiene la cámara mirando hacia el centro.
        /// </summary>
        private void MoveCamera()
        {
            Vector3 centerPoint = GetCenterPoint();
            Vector3 newPosition = centerPoint + offset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                newPosition,
                ref _velocity,
                smoothTime
            );

        transform.LookAt(centerPoint);
    }

        /// <summary>
        /// Ajusta el zoom de la cámara según la distancia entre objetivos.
        /// Calcula el field of view óptimo para mantener todos visibles.
        /// </summary>
        /// <remarks>
        /// Usa interpolación lineal entre minZoom y maxZoom.
        /// Aplica suavizado para transiciones suaves.
        /// </remarks>
        private void UpdateZoom()
        {
            float greatestDistance = GetGreatestDistance();
            float newZoom = Mathf.Lerp(maxZoom, minZoom, greatestDistance / zoomLimiter);

            _camera.fieldOfView = Mathf.SmoothDamp(
                _camera.fieldOfView,
                newZoom,
                ref _zoomVelocity,
                zoomSmoothTime
            );
        }

        /// <summary>
        /// Calcula el punto central entre todos los objetivos.
        /// </summary>
        /// <returns>Posición central calculada</returns>
        /// <remarks>
        /// Si solo hay un objetivo, retorna su posición directamente.
        /// Para múltiples objetivos, calcula el centro del bounds que los contiene.
        /// </remarks>
        private Vector3 GetCenterPoint()
        {
            if (targets.Count == 1)
                return targets[0].position;

            Bounds bounds = new Bounds(targets[0].position, Vector3.zero);

            for (int i = 0; i < targets.Count; i++)
            {
                bounds.Encapsulate(targets[i].position);
            }

            return bounds.center;
        }

        /// <summary>
        /// Calcula la mayor distancia entre todos los objetivos.
        /// </summary>
        /// <returns>Magnitud del tamaño del bounds que contiene todos los objetivos</returns>
        /// <remarks>
        /// Utilizado para determinar el nivel de zoom necesario.
        /// Calcula el bounds que engloba todas las posiciones.
        /// </remarks>
        private float GetGreatestDistance()
        {
            Bounds bounds = new Bounds(targets[0].position, Vector3.zero);

            for (int i = 0; i < targets.Count; i++)
            {
                bounds.Encapsulate(targets[i].position);
            }

            return bounds.size.magnitude;
        }
}
