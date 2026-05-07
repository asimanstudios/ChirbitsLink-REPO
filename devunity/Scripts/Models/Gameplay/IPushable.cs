using UnityEngine;

namespace ChibiCocina.Models
{
    /// <summary>
    /// Interfaz para cualquier objeto que pueda ser empujado o recibir fuerzas externas.
    /// Define el contrato para objetos afectados por física externa.
    /// </summary>
    /// <remarks>
    /// Implementada por jugadores y objetos interactuables.
    /// Utilizada en sistemas de combate y física.
    /// Permite aplicación controlada de fuerzas.
    /// </remarks>
    public interface IPushable
    {
        /// <summary>
        /// Aplica una fuerza de empuje al objeto.
        /// La fuerza puede tener duración limitada o instantánea.
        /// </summary>
        /// <param name="force">Vector de fuerza a aplicar</param>
        /// <param name="duration">Duración del efecto de empuje en segundos</param>
        void ApplyPush(Vector3 force, float duration);
    }
}
