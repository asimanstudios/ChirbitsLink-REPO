using UnityEngine;
using TMPro;

namespace ChibitsLink.UI.Minigames
{
    /// <summary>
    /// HUD flotante en mundo que muestra el tiempo restante de la bomba sobre el portador.
    /// Vive como hijo del prefab de la bomba, siguiendo al jugador automáticamente.
    /// Maneja orientación billboard y actualizaciones de texto.
    /// </summary>
    /// <remarks>
    /// Utiliza TextMeshPro para renderizado de texto.
    /// Incluye animación de aparición y cambio de color urgente.
    /// Se auto-conecta con componentes TextMeshPro hijos.
    /// </remarks>
    public class BombFloatingHUD : MonoBehaviour
    {
        [Header("Textos")]
        /// <summary>Texto del tiempo (ej: "12")</summary>
        public TextMeshPro timeText;
        /// <summary>Texto del icono (ej: "💣")</summary>
        public TextMeshPro iconText;

        [Header("Colores")]
        /// <summary>Color normal del texto</summary>
        public Color normalColor = Color.white;
        /// <summary>Color urgente del texto</summary>
        public Color urgentColor = Color.red;
        /// <summary>Umbral de segundos para color urgente</summary>
        [Tooltip("Segundos restantes en los que el texto cambia a color urgente.")]
        public float urgentThreshold = 5f;

        [Header("Animación de Aparición (Scale Punch)")]
        /// <summary>Duración de la animación de aparición</summary>
        public float appearanceDuration = 0.25f;
        /// <summary>Escala máxima durante aparición</summary>
        public float maxAppearanceScale = 1.4f;

        [Header("Posicionamiento")]
        /// <summary>Altura sobre la bomba</summary>
        public float heightAboveBomb = 0.6f;

        /// <summary>Cámara principal para billboard</summary>
        private Camera _mainCamera;
        /// <summary>Escala original del objeto</summary>
        private Vector3 _originalScale;
        /// <summary>Temporizador de animación de aparición</summary>
        private float _appearanceTimer = 0f;
        /// <summary>Indica si está animando</summary>
        private bool _isAnimating = false;

        /// <summary>
        /// Inicialización del HUD flotante.
        /// Busca cámara y configura escala original.
        /// </summary>
        private void Awake()
        {
            FindCamera();
            _originalScale = transform.localScale;

            // Smart Auto-Connector
            if (timeText == null || iconText == null)
            {
                var allTexts = GetComponentsInChildren<TextMeshPro>();
                string name;
                
                foreach (var t in allTexts)
                {
                    name = t.name.ToLower();
                    if (timeText == null && (name.Contains("time") || name.Contains("timer") || name.Contains("tiempo"))) 
                        timeText = t;

                    bool shouldAssignIcon = iconText == null && (name.Contains("icon") || name.Contains("emoji") || name.Contains("bomb"));
                    if (shouldAssignIcon) 
                        iconText = t;
                }
            }

            StartAppearanceAnimation();
        }

        private void Start()
        {
            UpdatePosition();
            FindCamera();
            if (_mainCamera != null)
            {
                // Inverse LookAt for perfect UI Billboard
                transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
            }
        }

        private void Update()
        {
            UpdateTimeText();
            UpdateBillboardOrientation();
            ProcessAppearanceAnimation();
        }

        /// <summary>
        /// Busca la cámara principal para orientación billboard.
        /// </summary>
        private void FindCamera()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                _mainCamera = FindObjectOfType<Camera>();
            }
        }

        private void UpdatePosition()
        {
            transform.localPosition = new Vector3(0, heightAboveBomb, 0);
        }

        private void UpdateBillboardOrientation()
        {
            if (_mainCamera != null)
            {
                // Inverse LookAt for perfect UI Billboard
                transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position);
            }
        }

        private void UpdateTimeText()
        {
            bool canUpdateText = timeText != null && BombTagGameManager.Instance != null;
            if (canUpdateText)
            {
                float t = BombTagGameManager.Instance.remainingBombTime;
                int seconds = Mathf.CeilToInt(t);
                timeText.text = seconds > 0 ? seconds.ToString() : "💥";

                bool urgent = t <= urgentThreshold;
                Color c = urgent ? urgentColor : normalColor;
                timeText.color = c;
                if (iconText != null) iconText.color = c;
            }
        }

        private void StartAppearanceAnimation()
        {
            _appearanceTimer = 0f;
            _isAnimating = true;
            transform.localScale = _originalScale * maxAppearanceScale;
        }

        private void ProcessAppearanceAnimation()
        {
            if (_isAnimating)
            {
                _appearanceTimer += Time.deltaTime;
                float progress = _appearanceTimer / appearanceDuration;
                
                if (progress >= 1f)
                {
                    _isAnimating = false;
                    transform.localScale = _originalScale;
                }
                else
                {
                    // Smooth scale back to original
                    float easedProgress = 1f - Mathf.Pow(1f - progress, 3f); // Ease-out cubic
                    float currentScale = Mathf.Lerp(maxAppearanceScale, 1f, easedProgress);
                    transform.localScale = _originalScale * currentScale;
                }
            }
        }
    }
}
