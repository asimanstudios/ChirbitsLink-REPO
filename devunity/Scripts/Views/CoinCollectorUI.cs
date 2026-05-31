using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.UI.Minigames
{
    /// <summary>
    /// Interfaz de usuario para el minijuego Coin Collector.
    /// Muestra HUD en juego, temporizador, puntuaciones y resultados.
    /// Se conecta automáticamente con el gestor del juego.
    /// </summary>
    /// <remarks>
    /// Maneja múltiples paneles UI (HUD y resultados).
    /// Proporciona actualización en tiempo real de puntuaciones.
    /// Incluye manejo de errores para componentes faltantes.
    /// </remarks>
    public class CoinCollectorUI : MonoBehaviour
    {
        [Header("Componentes del HUD")]
        /// <summary>Texto del temporizador</summary>
        public TextMeshProUGUI timerText;
        /// <summary>Texto de puntuación global</summary>
        public TextMeshProUGUI globalScoreText;
        /// <summary>Texto de cuenta regresiva</summary>
        public TextMeshProUGUI countdownText;
        /// <summary>Panel del HUD</summary>
        public GameObject hudPanel;

        [Header("Pantalla de Resultados")]
        /// <summary>Panel de resultados</summary>
        public GameObject resultsPanel;
        /// <summary>Texto de ranking</summary>
        public TextMeshProUGUI rankingText;

        /// <summary>Referencia al gestor del juego</summary>
        private CoinCollectorGameManager _gameManager;

        /// <summary>
        /// Inicialización de la UI.
        /// Configura componentes y establece estado inicial.
        /// </summary>
        private void Start()
        {
            try
            {
                InitializeComponents();
                SetupUI();
            }
            catch (ComponentNotFoundException ex)
            {
                Debug.LogError($"[CoinCollectorUI] Failed to initialize: {ex.Message}");
                throw new ComponentNotFoundException("CoinCollectorUI initialization failed", ex);
            }
            catch (System.NullReferenceException ex)
            {
                Debug.LogError($"[CoinCollectorUI] Null reference during initialization: {ex.Message}");
                throw new ComponentNotFoundException("Failed to initialize CoinCollectorUI due to null reference", ex);
            }
        }
        
        /// <summary>
        /// Inicializa los componentes necesarios.
        /// Busca el gestor del juego y valida referencias.
        /// </summary>
        /// <exception cref="ComponentNotFoundException">Si no se encuentra el gestor</exception>
        private void InitializeComponents()
        {
            _gameManager = CoinCollectorGameManager.Instance;
            if (_gameManager == null)
            {
                throw new ComponentNotFoundException("CoinCollectorGameManager not found");
            }
        }
        
        /// <summary>
        /// Configura el estado inicial de la UI.
        /// Oculta paneles no necesarios al inicio.
        /// </summary>
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
            if (timerText != null)
            {
                float time = _gameManager.remainingTime;
                int minutes = Mathf.FloorToInt(time / 60);
                int seconds = Mathf.FloorToInt(time % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

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

                string prefix;
                string uid;
                string name;
                int lvl;

                for (int i = 0; i < ranking.Count; i++)
                {
                    prefix = (i == 0) ? "👑 " : (i + 1) + ". ";
                    uid = ranking[i].Key;
                    name = uid;
                    lvl = 1;
                    
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
