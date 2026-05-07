using UnityEngine;
using TMPro;
using System.Text;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.UI.Minigames
{
    public class HookPartyUI : MonoBehaviour
    {
        [Header("HUD Components")]
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI globalScoreText;
        public TextMeshProUGUI countdownText;
        public GameObject hudPanel;

        [Header("Results Screen")]
        public GameObject resultsPanel;
        public TextMeshProUGUI rankingText;

        private HookPartyManager _gameManager;

        private void Start()
        {
            try
            {
                InitializeComponents();
                SetupUI();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[HookPartyUI] Failed to initialize: {ex.Message}");
                throw new ComponentNotFoundException("Failed to initialize HookPartyUI", ex);
            }
        }
        
        private void InitializeComponents()
        {
            _gameManager = HookPartyManager.Instance;
            if (_gameManager == null)
            {
                throw new ComponentNotFoundException("HookPartyManager not found");
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
            if (_gameManager != null)
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
                float time = _gameManager.TimeRemaining;
                int minutes = Mathf.FloorToInt(time / 60);
                int seconds = Mathf.FloorToInt(time % 60);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            // Score
            UpdateRankingHUD();
        }

        private void ActualizarRankingHUD()
        {
            bool canUpdateScoreHud = textoPuntuacionGlobal != null && ScoreManager.Instance != null;
            if (canUpdateScoreHud)
            {
                var scores = ScoreManager.Instance.GetAllScores();
                
                // Ordenar por puntuación 
                var ranking = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, int>>(scores);
                ranking.Sort((a, b) => b.Value.CompareTo(a.Value));

                StringBuilder sb = new StringBuilder();

                if (ranking.Count == 0)
                {
                    sb.Append("¡Agárrate FUERTE!");
                }
                else
                {
                    string uid;
                    string nombre;
                    int lvl;
                    
                    foreach (var entry in ranking)
                    {
                        uid = entry.Key;
                        nombre = uid;
                        lvl = 1;

                        if (ChibitsLink.GameSide.PlayerManager.Instance != null)
                        {
                            nombre = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerName(uid);
                            lvl = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerLevel(uid);
                        }
                        
                        sb.Append($"[Lvl {lvl}] {nombre}: <color=yellow>{entry.Value}</color>   ");
                    }
                }

                textoPuntuacionGlobal.text = sb.ToString();
            }
        }

        private void ActualizarEstadoVisual()
        {
            var gestor = HookPartyManager.Instance;
            HookPartyState estado = gestor.CurrentState;

            // Cuenta Atrás
            if (estado == HookPartyState.Countdown)
            {
                if (textoCuentaAtras != null)
                {
                    textoCuentaAtras.gameObject.SetActive(true);
                    textoCuentaAtras.text = gestor.CountdownValue.ToString();
                }
            }
            else
            {
                if (textoCuentaAtras != null) textoCuentaAtras.gameObject.SetActive(false);
            }

            // HUD: Visible desde el inicio (Preparación, Cuenta atrás e InGame)
            bool mostrarHUD = (estado == HookPartyState.Preparing || estado == HookPartyState.Countdown || estado == HookPartyState.InGame);
            if (panelHUD != null) panelHUD.SetActive(mostrarHUD);

            // Resultados: Solo al terminar o en transición
            bool mostrarResultados = (estado == HookPartyState.Finished || estado == HookPartyState.TransitioningToLobby);
            if (panelResultados != null)
            {
                if (mostrarResultados)
                {
                    if (!panelResultados.activeSelf || estado == HookPartyState.TransitioningToLobby)
                    {
                        MostrarResultados();
                    }
                }
                else
                {
                    panelResultados.SetActive(false);
                }
            }
        }

        public void MostrarResultados()
        {
            bool canShowResults = panelResultados != null && ScoreManager.Instance != null;
            if (canShowResults)
            {
                panelResultados.SetActive(true);

                var scores = ScoreManager.Instance.GetAllScores();
                var ranking = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string, int>>(scores);
                ranking.Sort((a, b) => b.Value.CompareTo(a.Value));

                StringBuilder sb = new StringBuilder();
                
                var gestor = HookPartyManager.Instance;
                if (gestor.CurrentState == HookPartyState.TransitioningToLobby)
                {
                    sb.AppendLine($"<color=orange>Desenganchando en... {gestor.ReturnTimeRemaining:0}</color>\n");
                }

                sb.AppendLine("<size=120%>SALDO DE LA FIESTA</size>\n");

                for (int i = 0; i < ranking.Count; i++)
                {
                    string prefijo = (i == 0) ? "🏆 " : (i + 1) + ". ";
                    string uid = ranking[i].Key;
                    string nombre = uid;
                    int lvl = 1;
                    
                    if (ChibitsLink.GameSide.PlayerManager.Instance != null)
                    {
                        nombre = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerName(uid);
                        lvl = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerLevel(uid);
                    }

                    sb.AppendLine($"{prefijo}[Lvl {lvl}] {nombre}: {ranking[i].Value} puntos");
                }

                if (textoRanking != null) textoRanking.text = sb.ToString();
            }
        }
    }
}
