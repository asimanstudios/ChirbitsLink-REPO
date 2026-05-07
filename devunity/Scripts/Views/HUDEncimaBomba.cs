using UnityEngine;
using TMPro;

namespace ChibitsLink.UI.Minigames
{
    /// <summary>
    /// World-space floating HUD showing the remaining bomb time above the carrier.
    /// Lives as a child of the Bomb Prefab, so it follows the player automatically.
    /// Handles billboard orientation and text updates.
    /// </summary>
    public class BombFloatingHUD : MonoBehaviour
    {
        [Header("Texts")]
        public TextMeshPro timeText;   // Ej: "12"
        public TextMeshPro iconText;   // Ej: "💣"

        [Header("Colors")]
        public Color normalColor = Color.white;
        public Color urgentColor = Color.red;
        [Tooltip("Seconds remaining at which the text changes to urgent color.")]
        public float urgentThreshold = 5f;

        [Header("Appearance Animation (Scale Punch)")]
        public float appearanceDuration = 0.25f;
        public float maxAppearanceScale = 1.4f;

        [Header("Positioning")]
        public float heightAboveBomb = 0.6f;

        private Camera _mainCamera;
        private Vector3 _originalScale;
        private float _appearanceTimer = 0f;
        private bool _isAnimating = false;

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
