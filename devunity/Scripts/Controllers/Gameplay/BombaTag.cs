using UnityEngine;

namespace ChibiCocina.BombTag
{
    /// <summary>
    /// Plantilla de datos para configuración de bombas - SIN LÓGICA.
    /// Configurar en la escena para proporcionar información de bombas al gestor.
    /// </summary>
    /// <remarks>
    /// Este componente solo almacena datos de configuración.
    /// No contiene lógica de juego, solo parámetros.
    /// </remarks>
    public class BombaTag : MonoBehaviour
    {
        [Header("Datos de Plantilla")]
        /// <summary>Prefab de la bomba a instanciar</summary>
        public GameObject bombPrefab;
        /// <summary>Efecto visual de explosión</summary>
        public GameObject explosionVFX;
        /// <summary>Sonido de explosión</summary>
        public AudioClip explosionSFX;
        /// <summary>Sonido de tic-tac de la bomba</summary>
        public AudioClip tickSFX;

        [Header("Configuración")]
        /// <summary>Duración de la bomba antes de explotar (segundos)</summary>
        public float bombDuration = 15f;
        /// <summary>Desplazamiento vertical de la bomba sobre el jugador</summary>
        public float verticalOffset = 2.0f;
        /// <summary>Color de parpadeo cuando está por explotar</summary>
        public Color flashColor = Color.red;
    }
}
