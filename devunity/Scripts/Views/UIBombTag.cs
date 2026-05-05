using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;

namespace ChibiCocina.BombTag
{
    /// <summary>
    /// Global HUD for the BombTag minigame. Fetches info directly from GestorBombTag.
    /// </summary>
    public class UIBombTag : MonoBehaviour
    {
        [Header("In-Game HUD")]
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI carrierText;
        public TextMeshProUGUI aliveText;
        public TextMeshProUGUI countdownText;
        public GameObject hudPanel;

        [Header("Results Screen")]
        public GameObject resultsPanel;
        public TextMeshProUGUI rankingText;

        private void Start()
        {
            // --- Smart Auto-Connector ---
            if (countdownText == null || hudPanel == null || resultsPanel == null)
            {
                var allTransforms = GetComponentsInChildren<Transform>(true);
                var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

                foreach (var t in allTransforms)
                {
                    string n = t.name.ToLower();
                    if (hudPanel == null && (n.Contains("hud") || n.Contains("ingame"))) hudPanel = t.gameObject;
                    if (resultsPanel == null && (n.Contains("result") || n.Contains("final"))) resultsPanel = t.gameObject;
                }

                foreach (var txt in allTexts)
                {
                    string n = txt.name.ToLower();
                    if (countdownText == null && (n.Contains("count") || n.Contains("cuenta"))) countdownText = txt;
                }
            }

            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (countdownText != null) 
            {
                countdownText.transform.SetAsLastSibling();
            }
        }

        private void Update()
        {
            if (GestorBombTag.Instance != null)
            {
                UpdateHUD();
                UpdateVisualState();
            }
        }

        private void UpdateHUD()
        {
            var manager = GestorBombTag.Instance;

            // Timer MM:SS
            if (timerText != null)
            {
                float t = manager.remainingBombTime;
                int min = Mathf.FloorToInt(t / 60);
                int seg = Mathf.FloorToInt(t % 60);
                timerText.text = $"{min:00}:{seg:00}";
                timerText.color = t <= 5f ? Color.red : Color.black;
            }

            // Carrier
            if (carrierText != null)
            {
                carrierText.text = $"WITH BOMB: <color=yellow>{manager.GetCarrierName()}</color>";
            }

            // Alive
            if (aliveText != null)
            {
                aliveText.text = $"Alive: {manager.GetAliveCount()}";
            }
        }

        private void UpdateVisualState()
        {
            var manager = GestorBombTag.Instance;
            if (manager != null)
            {
                BombTagState state = manager.CurrentState;

                // --- Countdown Visibility ---
                bool showCountdown = (state == BombTagState.Preparing || state == BombTagState.Countdown);
                if (countdownText != null)
                {
                    if (countdownText.gameObject.activeSelf != showCountdown) countdownText.gameObject.SetActive(showCountdown);
                    if (showCountdown)
                    {
                        countdownText.text = (state == BombTagState.Countdown) ? manager.currentCountdown.ToString() : "";
                    }
                }

                // --- HUD and Results Panels ---
                bool isOver = (state == BombTagState.Result || state == BombTagState.Ending);
                bool showHUD = !isOver;

                if (hudPanel != null && hudPanel.activeSelf != showHUD) hudPanel.SetActive(showHUD);
                
                if (resultsPanel != null)
                {
                    if (resultsPanel.activeSelf != isOver)
                    {
                        resultsPanel.SetActive(isOver);
                        if (isOver) ShowResults(); // Initial draw
                    }
                    
                    // ONLY update timer during Ending state, don't rebuild everything if not needed
                    if (state == BombTagState.Ending) ShowResults(); 
                }
            }
        }

        private string lastResultsText = "";
        private void ShowResults()
        {
            if (rankingText != null)
            {
                var manager = GestorBombTag.Instance;
                StringBuilder sb = new StringBuilder();

                if (manager.CurrentState == BombTagState.Ending)
                {
                    sb.AppendLine($"<color=orange>Returning to lobby in... {manager.resultTimeRemaining:0}</color>\n");
                }

                // Optimization: Only rebuild full list if text changed significantly or it's the first time
                sb.AppendLine("<size=120%>GAME OVER!</size>\n");

                // Winners
                var winners = manager.GetWinners();
                foreach (var w in winners) 
                {
                    if (w == null) continue;
                    string name = manager.GetPlayerName(w);
                    sb.AppendLine($"🥇 WINNER: <color=yellow>{name}</color>");
                }

                sb.AppendLine("\n<b>ELIMINATION ORDER:</b>");
                var elims = manager.GetEliminationOrder();
                // Start from the first eliminated to the last
                for (int i = 0; i < elims.Count; i++)
                {
                    if (elims[i] == null) continue;
                    string name = manager.GetPlayerName(elims[i]);
                    int pos = elims.Count - i; // Convert index to rank (e.g. 4th, 3rd...)
                    sb.AppendLine($"<color=white>#{pos}</color> {name}");
                }

                string finalStr = sb.ToString();
                if (lastResultsText != finalStr)
                {
                    lastResultsText = finalStr;
                    rankingText.text = finalStr;
                }
            }
        }
    }
}
