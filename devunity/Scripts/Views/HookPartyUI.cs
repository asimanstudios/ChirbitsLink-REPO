using UnityEngine;
using TMPro;
using System.Text;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.UI.Minigames
{
    /// <summary>
    /// Interfaz de usuario para el minijuego Hook Party.
    /// Muestra HUD en juego, temporizador, puntuaciones y resultados.
    /// Se conecta automáticamente con el gestor del juego.
    /// </summary>
    /// <remarks>
    /// Maneja múltiples paneles UI (HUD y resultados).
    /// Proporciona actualización en tiempo real de puntuaciones.
    /// Incluye manejo de errores para componentes faltantes.
    /// </remarks>
    public class HookPartyUI : MonoBehaviour
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
        private HookPartyManager _gameManager;

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
                Debug.LogError($"[HookPartyUI] Failed to initialize: {ex.Message}");
                throw new ComponentNotFoundException("HookPartyUI initialization failed", ex);
            }
            catch (System.NullReferenceException ex)
            {
                Debug.LogError($"[HookPartyUI] Null reference during initialization: {ex.Message}");
                throw new ComponentNotFoundException("Failed to initialize HookPartyUI due to null reference", ex);
            }
        }
        
        /// <summary>
        /// Inicializa los componentes necesarios.
        /// Busca el gestor del juego y valida referencias.
        /// </summary>
        /// <exception cref="ComponentNotFoundException">Si no se encuentra el gestor</exception>
        private void InitializeComponents()
        {
            _gameManager = HookPartyManager.Instance;
            if (_gameManager == null)
            {
                throw new ComponentNotFoundException("HookPartyManager not found");
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

        /// <summary>
        /// Actualización de la UI cada frame.
        /// Actualiza HUD y estado visual según el estado del juego.
        /// </summary>
        private void Update()
        {
            if (_gameManager != null)
            {
                UpdateHUD();
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Actualiza el HUD del juego.
        /// Muestra temporizador y puntuaciones en tiempo real.
        /// </summary>
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

        /// <summary>
        /// Actualiza el HUD de puntuaciones.
        /// Muestra ranking de jugadores con nombres y niveles.
        /// </summary>
        private void UpdateRankingHUD()
        {
            bool canUpdateScoreHud = globalScoreText != null && ScoreManager.Instance != null;
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

                globalScoreText.text = sb.ToString();
            }
        }

        /// <summary>
        /// Actualiza el estado visual de la UI.
        /// Muestra/oculta paneles según el estado del juego.
        /// </summary>
        private void UpdateVisualState()
        {
            var gestor = HookPartyManager.Instance;
            HookPartyState estado = gestor.CurrentState;

            // Cuenta Atrás
            if (estado == HookPartyState.Countdown)
            {
                if (countdownText != null)
                {
                    countdownText.gameObject.SetActive(true);
                    countdownText.text = gestor.CountdownValue.ToString();
                }
            }
            else
            {
                if (countdownText != null) countdownText.gameObject.SetActive(false);
            }

            // HUD: Visible desde el inicio (Preparación, Cuenta atrás e InGame)
            bool mostrarHUD = (estado == HookPartyState.Preparing || estado == HookPartyState.Countdown || estado == HookPartyState.InGame);
            if (hudPanel != null) hudPanel.SetActive(mostrarHUD);

            // Resultados: Solo al terminar o en transición
            bool mostrarResultados = (estado == HookPartyState.Finished || estado == HookPartyState.TransitioningToLobby);
            if (resultsPanel != null)
            {
                if (mostrarResultados)
                {
                    if (!resultsPanel.activeSelf || estado == HookPartyState.TransitioningToLobby)
                    {
                        MostrarResultados();
                    }
                }
                else
                {
                    resultsPanel.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Muestra la pantalla de resultados.
        /// Despliega ranking final y mensaje de despedida.
        /// </summary>
        public void MostrarResultados()
        {
            bool canShowResults = resultsPanel != null && ScoreManager.Instance != null;
            if (canShowResults)
            {
                resultsPanel.SetActive(true);

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

                string prefijo;
                string uid;
                string nombre;
                int lvl;

                for (int i = 0; i < ranking.Count; i++)
                {
                    prefijo = (i == 0) ? "🏆 " : (i + 1) + ". ";
                    uid = ranking[i].Key;
                    nombre = uid;
                    lvl = 1;
                    
                    if (ChibitsLink.GameSide.PlayerManager.Instance != null)
                    {
                        nombre = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerName(uid);
                        lvl = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerLevel(uid);
                    }

                    sb.AppendLine($"{prefijo}[Lvl {lvl}] {nombre}: {ranking[i].Value} puntos");
                }

                if (rankingText != null) rankingText.text = sb.ToString();
            }
        }
    }
}
