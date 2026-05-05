using UnityEngine;
using TMPro;

namespace ChibiCocina.BombTag
{
    /// <summary>
    /// World-space floating HUD showing the remaining bomb time above the carrier.
    /// Lives as a child of the Bomb Prefab, so it follows the player automatically.
    /// Handles billboard orientation and text updates.
    /// </summary>
    public class HUDEncimaBomba : MonoBehaviour
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

        private Camera mainCam;
        private Vector3 originalScale;
        private float appearanceTimer = 0f;
        private bool isAnimating = false;

        private void Awake()
        {
            FindCamera();
            originalScale = transform.localScale;

            // Smart Auto-Connector
            if (timeText == null || iconText == null)
            {
                var allTexts = GetComponentsInChildren<TextMeshPro>();
                foreach (var t in allTexts)
                {
                    string n = t.name.ToLower();
                    if (timeText == null && (n.Contains("time") || n.Contains("timer") || n.Contains("tiempo"))) 
                        timeText = t;

                    bool shouldAssignIcon = iconText == null && (n.Contains("icon") || n.Contains("emoji") || n.Contains("bomb"));
                    if (shouldAssignIcon) 
                        iconText = t;
                }
            }

            StartAppearanceAnimation();
        }

        private void LateUpdate()
        {
            MaintainRelativePosition();
            OrientTowardsCamera();
            UpdateTimeText();
            ProcessAppearanceAnimation();
        }

        private void FindCamera()
        {
            if (mainCam == null)
            {
                mainCam = Camera.main;
                if (mainCam == null)
                {
                    mainCam = FindFirstObjectByType<Camera>();
                }
            }
        }

        private void MaintainRelativePosition()
        {
            // Forces local position to stay consistent relative to the bomb model
            transform.localPosition = new Vector3(0, heightAboveBomb, 0);
        }

        private void OrientTowardsCamera()
        {
            FindCamera();
            if (mainCam != null)
            {
                // Inverse LookAt for perfect UI Billboard
                transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
            }
        }

        private void UpdateTimeText()
        {
            bool canUpdateText = timeText != null && GestorBombTag.Instance != null;
            if (canUpdateText)
            {
                float t = GestorBombTag.Instance.remainingBombTime;
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
            appearanceTimer = 0f;
            isAnimating = true;
            transform.localScale = originalScale * maxAppearanceScale;
        }

        private void ProcessAppearanceAnimation()
        {
            if (isAnimating)
            {
                appearanceTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(appearanceTimer / appearanceDuration);
                transform.localScale = Vector3.Lerp(originalScale * maxAppearanceScale, originalScale, progress);

                if (progress >= 1f)
                {
                    transform.localScale = originalScale;
                    isAnimating = false;
                }
            }
        }
    }
}
