using UnityEngine;

namespace ChibiCocina.Models
{
    /// <summary>
    /// Interfaz para cualquier objeto que pueda ser empujado o recibir fuerzas externas.
    /// </summary>
    public interface IPushable
    {
        /// <summary>
        /// Aplica una fuerza de empuje al objeto.
        /// </summary>
        /// <param name="force">Vector de fuerza a aplicar.</param>
        /// <param name="duration">Duración del efecto de empuje.</param>
        void ApplyPush(Vector3 force, float duration);
    }
}
