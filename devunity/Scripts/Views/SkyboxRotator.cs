using UnityEngine;

namespace ChibitsLink.Views
{
    /// <summary>
    /// Controlador para rotación del skybox.
    /// Rota el skybox continuamente para efecto visual dinámico.
    /// </summary>
    /// <remarks>
    /// Utilizado para crear movimiento en el fondo del cielo.
    /// Ajusta el shader del skybox con rotación continua.
    /// </remarks>
    public class SkyboxRotator : MonoBehaviour
    {
        [Header("Configuración de Rotación")]
        /// <summary>Velocidad de rotación del skybox</summary>
        public float rotationSpeed = 10f;
        
        /// <summary>Rotación actual acumulada</summary>
        private float _currentRotation = 0f;

        /// <summary>
        /// Actualiza la rotación del skybox cada frame.
        /// Aplica la rotación al shader del skybox.
        /// </summary>
        private void Update()
        {
            _currentRotation += rotationSpeed * Time.deltaTime;
            RenderSettings.skybox.SetFloat("_Rotation", _currentRotation);
        }
    }
}
