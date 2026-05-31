using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;
using ChibitsLink.Core.Exceptions;

namespace ChibitsLink.UI.Minigames
{
    /// <summary>
    /// Interfaz de usuario principal para el minijuego BombTag.
    /// Muestra información en tiempo real y resultados finales del juego.
    /// Obtiene datos directamente del BombTagGameManager.
    /// </summary>
    /// <remarks>
    /// Implementa auto-conexión de elementos UI para facilitar configuración.
    /// Maneja estados visuales según el estado actual del juego.
    /// </remarks>
    public class BombTagUI : MonoBehaviour
    {
        [Header("In-Game HUD")]
        /// <summary>Texto para mostrar el temporizador de la bomba</summary>
        public TextMeshProUGUI timerText;
        /// <summary>Texto para mostrar el portador actual de la bomba</summary>
        public TextMeshProUGUI carrierText;
        /// <summary>Texto para mostrar la cantidad de jugadores vivos</summary>
        public TextMeshProUGUI aliveText;
        /// <summary>Texto para mostrar la cuenta regresiva inicial</summary>
        public TextMeshProUGUI countdownText;
        /// <summary>Panel principal del HUD durante el juego</summary>
        public GameObject hudPanel;

        [Header("Results Screen")]
        /// <summary>Panel para mostrar resultados finales</summary>
        public GameObject resultsPanel;
        /// <summary>Texto para mostrar ranking y resultados</summary>
        public TextMeshProUGUI rankingText;

        /// <summary>Referencia al gestor del juego BombTag</summary>
        private BombTagGameManager _gameManager;
        /// <summary>Cache del último texto de resultados para optimización</summary>
        private string lastResultsText = "";

        /// <summary>
        /// Genera el texto de resultados para mostrar.
        /// Crea ranking con información detallada de jugadores.
        /// </summary>
        /// <returns>String con el ranking formateado</returns>
        private string GenerateResultsText() { return ""; }

        /// <summary>
        /// Inicializa los componentes y configura la interfaz.
        /// Realiza validación y auto-conexión de elementos.
        /// </summary>
        /// <exception cref="ComponentNotFoundException">Si falla la inicialización</exception>
        private void Start()
        {
            try
            {
                InitializeComponents();
                SetupUI();
            }
            catch (ComponentNotFoundException ex)
            {
                Debug.LogError($"[BombTagUI] Failed to initialize: {ex.Message}");
                throw new ComponentNotFoundException("BombTagUI initialization failed", ex);
            }
            catch (System.NullReferenceException ex)
            {
                Debug.LogError($"[BombTagUI] Null reference during initialization: {ex.Message}");
                throw new ComponentNotFoundException("Failed to initialize BombTagUI due to null reference", ex);
            }
        }
        
        /// <summary>
        /// Inicializa los componentes requeridos por la UI.
        /// Obtiene referencias al gestor del juego y conecta elementos UI.
        /// </summary>
        /// <exception cref="ComponentNotFoundException">Si no se encuentra BombTagGameManager</exception>
        private void InitializeComponents()
        {
            _gameManager = BombTagGameManager.Instance;
            if (_gameManager == null)
            {
                throw new ComponentNotFoundException("BombTagGameManager not found");
            }
            
            // Smart Auto-Connector
            AutoConnectUIElements();
        }
        
        /// <summary>
        /// Auto-conecta elementos UI basándose en nombres.
        /// Busca automáticamente elementos no asignados en el inspector.
        /// </summary>
        /// <remarks>
        /// Busca por nombres que contienen palabras clave específicas.
        /// Facilita la configuración sin necesidad de asignar manualmente.
        /// </remarks>
        private void AutoConnectUIElements()
        {
            if (countdownText == null || hudPanel == null || resultsPanel == null)
            {
                var allTransforms = GetComponentsInChildren<Transform>(true);
                var allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

                string name;
                foreach (var transform in allTransforms)
                {
                    name = transform.name.ToLower();
                    if (hudPanel == null && (name.Contains("hud") || name.Contains("ingame"))) 
                    {
                        hudPanel = transform.gameObject;
                    }
                    
                    if (resultsPanel == null && (name.Contains("result") || name.Contains("final"))) 
                    {
                        resultsPanel = transform.gameObject;
                    }
                }

                foreach (var text in allTexts)
                {
                    name = text.name.ToLower();
                    if (countdownText == null && (name.Contains("count") || name.Contains("cuenta"))) 
                    {
                        countdownText = text;
                    }
                }
            }
        }
        
        /// <summary>
        /// Configura el estado inicial de la UI.
        /// Oculta paneles no necesarios y ajusta orden de elementos.
        /// </summary>
        private void SetupUI()
        {
            if (resultsPanel != null) 
            {
                resultsPanel.SetActive(false);
            }
            
            if (countdownText != null) 
            {
                countdownText.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// Actualiza la interfaz cada frame.
        /// Refresca el HUD y el estado visual según el juego.
        /// </summary>
        /// <remarks>
        /// Solo actualiza si existe una instancia del gestor del juego.
        /// </remarks>
        /// <summary>
        /// Actualiza la interfaz de usuario cada frame.
        /// Sincroniza el HUD con el estado del juego.
        /// </summary>
        private void Update()
        {
            if (BombTagGameManager.Instance != null)
            {
                UpdateHUD();
                UpdateVisualState();
            }
        }

        /// <summary>
        /// Actualiza los elementos del HUD en tiempo real.
        /// Refresca temporizador, portador y cantidad de jugadores vivos.
        /// </summary>
        /// <summary>
        /// Actualiza el HUD del juego.
        /// Muestra temporizador, portador y jugadores vivos.
        /// </summary>
        private void UpdateHUD()
        {
            var manager = BombTagGameManager.Instance;

            if (timerText != null)
            {
                float t = manager.remainingBombTime;
                int min = Mathf.FloorToInt(t / 60);
                int seg = Mathf.FloorToInt(t % 60);
                timerText.text = $"{min:00}:{seg:00}";
                timerText.color = t <= 5f ? Color.red : Color.black;
            }

            if (carrierText != null)
            {
                carrierText.text = $"WITH BOMB: <color=yellow>{manager.GetCarrierName()}</color>";
            }

            if (aliveText != null)
            {
                aliveText.text = $"Alive: {manager.GetAliveCount()}";
            }
        }

        /// <summary>
        /// Actualiza el estado visual de la UI según el estado del juego.
        /// Muestra/oculta paneles según la fase actual.
        /// </summary>
        /// <summary>
        /// Actualiza el estado visual de la UI.
        /// Muestra/oculta paneles según el estado del juego.
        /// </summary>
        private void UpdateVisualState()
        {
            var manager = BombTagGameManager.Instance;
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

        /// <summary>
        /// Muestra la pantalla de resultados finales.
        /// Construye y muestra el ranking de jugadores y orden de eliminación.
        /// </summary>
        /// <remarks>
        /// Optimizado para solo reconstruir cuando el contenido cambia.
        /// Incluye cuenta regresiva para retorno al lobby.
        /// </remarks>
        /// <summary>
        /// Muestra la pantalla de resultados.
        /// Genera y muestra el ranking final del juego.
        /// </summary>
        private void ShowResults()
        {
            if (rankingText != null)
            {
                var manager = BombTagGameManager.Instance;
                StringBuilder sb = new StringBuilder();

                if (manager.CurrentState == BombTagState.Ending)
                {
                    sb.AppendLine($"<color=orange>Returning to lobby in... {manager.resultTimeRemaining:0}</color>\n");
                }

                sb.AppendLine("<size=120%>GAME OVER!</size>\n");

                var winners = manager.GetWinners();
                string name;
                foreach (var w in winners) 
                {
                    if (w != null)
                    {
                        name = manager.GetPlayerName(w);
                        sb.AppendLine($"🥇 WINNER: <color=yellow>{name}</color>");
                    }
                }

                sb.AppendLine("\n<b>ELIMINATION ORDER:</b>");
                var elims = manager.GetEliminationOrder();
                int pos;
                // Start from the first eliminated to the last
                for (int i = 0; i < elims.Count; i++)
                {
                    if (elims[i] != null)
                    {
                        name = manager.GetPlayerName(elims[i]);
                        pos = elims.Count - i; // Convert index to rank (e.g. 4th, 3rd...)
                        sb.AppendLine($"<color=white>#{pos}</color> {name}");
                    }
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
