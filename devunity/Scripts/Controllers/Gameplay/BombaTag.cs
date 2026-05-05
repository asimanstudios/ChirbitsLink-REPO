using UnityEngine;

namespace ChibiCocina.BombTag
{
    /// <summary>
    /// DATA TEMPLATE ONLY - NO LOGIC.
    /// Configure this in the scene to provide the Gestor with bomb info.
    /// </summary>
    public class BombaTag : MonoBehaviour
    {
        [Header("Template Data")]
        public GameObject bombPrefab;
        public GameObject explosionVFX;
        public AudioClip explosionSFX;
        public AudioClip tickSFX;

        [Header("Settings")]
        public float bombDuration = 15f;
        public float verticalOffset = 2.0f;
        public Color flashColor = Color.red;
    }
}
