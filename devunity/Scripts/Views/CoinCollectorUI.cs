using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.UI.Minigames
{
    public class CoinCollectorUI : MonoBehaviour
    {
        [Header("HUD Components")]
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI globalScoreText;
        public TextMeshProUGUI countdownText;
        public GameObject hudPanel;

        [Header("Results Screen")]
        public GameObject resultsPanel;
        public TextMeshProUGUI rankingText;

        private CoinCollectorGameManager _gameManager;

        private void Start()
        {
            try
            {
                InitializeComponents();
                SetupUI();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CoinCollectorUI] Failed to initialize: {ex.Message}");
                throw new ComponentNotFoundException("Failed to initialize CoinCollectorUI", ex);
            }
        }
        
        private void InitializeComponents()
        {
            _gameManager = CoinCollectorGameManager.Instance;
            if (_gameManager == null)
            {
                throw new ComponentNotFoundException("CoinCollectorGameManager not found");
            }
        }
        
        private void SetupUI()
        {
            if (resultsPanel != null) 
            {
                resultsPanel.SetActive(false);
            }
            
            if (countdownText != null) 
            {
                countdownText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            bool hasManagerInstance = _gameManager != null;
            if (hasManagerInstance)
            {
                UpdateHUD();
                UpdateVisualState();
            }
        }

        private void UpdateHUD()
        {
            // Timer
            if (timerText != null)
            {
                float time = _gameManager.remainingTime;
                int minutes = Mathf.FloorToInt(time / 60);
                int seconds = Mathf.FloorToInt(time % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            // Score
            UpdateRankingHUD();
        }

        private string _cachedRankingText = "";
        private int _cachedScoreHash = 0;

        private void UpdateRankingHUD()
        {
            bool canUpdateRanking = globalScoreText != null && CoinCollectorGameManager.Instance != null;
            if (canUpdateRanking)
            {
                var ranking = CoinCollectorGameManager.Instance.GetRanking();
                
                // OPTIMIZATION: Only rebuild string if score hash has changed
                int currentHash = 0;
                foreach (var r in ranking) currentHash ^= r.Key.GetHashCode() ^ r.Value.GetHashCode();

                bool canUseCachedText = currentHash == _cachedScoreHash && !string.IsNullOrEmpty(_cachedRankingText);
                if (canUseCachedText)
                {
                    globalScoreText.text = _cachedRankingText;
                }
                else
                {
                    _cachedScoreHash = currentHash;
                    StringBuilder sb = new StringBuilder();

                    if (ranking.Count == 0)
                    {
                        sb.Append("¡A recoger monedas!");
                    }
                    else
                    {
                        string uid;
                        string name;
                        int lvl;
                        
                        foreach (var entry in ranking)
                        {
                            uid = entry.Key;
                            name = uid;
                            lvl = 1;

                            if (ChibitsLink.GameSide.PlayerManager.Instance != null)
                            {
                                name = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerName(uid);
                                lvl = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerLevel(uid);
                            }
                            
                            sb.Append($"[Lvl {lvl}] {name}: <color=yellow>{entry.Value}</color>   ");
                        }
                    }

                    _cachedRankingText = sb.ToString();
                    globalScoreText.text = _cachedRankingText;
                }
            }
        }

        private void UpdateVisualState()
        {
            var manager = CoinCollectorGameManager.Instance;
            GameState state = manager.CurrentState;

            // Countdown
            if (state == GameState.Countdown)
            {
                if (countdownText != null)
                {
                    countdownText.gameObject.SetActive(true);
                    countdownText.text = manager.countdownValue.ToString();
                }
            }
            else
            {
                if (countdownText != null) countdownText.gameObject.SetActive(false);
            }

            // HUD and Results
            if (state == GameState.InGame)
            {
                if (hudPanel != null) hudPanel.SetActive(true);
                if (resultsPanel != null) resultsPanel.SetActive(false);
            }

            bool isResultState = state == GameState.Finished || state == GameState.TransitioningToLobby;
            if (isResultState)
            {
                if (hudPanel != null) hudPanel.SetActive(false);
                if (resultsPanel != null && !resultsPanel.activeSelf)
                {
                    ShowResults();
                }
                
                // If we're in transition, reload points in results 
                // to show the return time counter
                if (state == GameState.TransitioningToLobby)
                {
                    ShowResults();
                }
            }
        }

        public void ShowResults()
        {
            if (resultsPanel != null)
            {
                resultsPanel.SetActive(true);

                var ranking = CoinCollectorGameManager.Instance.GetRanking();
                StringBuilder sb = new StringBuilder();
                
                var manager = CoinCollectorGameManager.Instance;
                if (manager.CurrentState == GameState.TransitioningToLobby)
                {
                    sb.AppendLine($"<color=orange>Returning to lobby in... {manager.returnTimeRemaining:0}</color>\n");
                }

                sb.AppendLine("<size=120%>RANKING FINAL</size>\n");

                for (int i = 0; i < ranking.Count; i++)
                {
                    string prefix = (i == 0) ? "👑 " : (i + 1) + ". ";
                    string uid = ranking[i].Key;
                    string name = uid;
                    int lvl = 1;
                    
                    if (ChibitsLink.GameSide.PlayerManager.Instance != null)
                    {
                        name = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerName(uid);
                        lvl = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerLevel(uid);
                    }

                    sb.AppendLine($"{prefix}[Lvl {lvl}] {name}: {ranking[i].Value} coins");
                }

                if (rankingText != null) rankingText.text = sb.ToString();
            }
        }
    }
}
