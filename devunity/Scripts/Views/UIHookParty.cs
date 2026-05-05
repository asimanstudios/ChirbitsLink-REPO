using UnityEngine;
using TMPro;
using System.Text;

namespace ChibitsLink.GameSide.HookParty
{
    public class UIHookParty : MonoBehaviour
    {
        [Header("HUD")]
        public TextMeshProUGUI textoTemporizador;
        public TextMeshProUGUI textoPuntuacionGlobal;
        public TextMeshProUGUI textoCuentaAtras;
        public GameObject panelHUD;

        [Header("Resultados")]
        public GameObject panelResultados;
        public TextMeshProUGUI textoRanking;

        private void Start()
        {
            if (panelResultados != null) panelResultados.SetActive(false);
            if (textoCuentaAtras != null) textoCuentaAtras.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (HookPartyManager.Instance != null)
            {
                ActualizarHUD();
                ActualizarEstadoVisual();
            }
        }

        private void ActualizarHUD()
        {
            var gestor = HookPartyManager.Instance;

            // Timer
            if (textoTemporizador != null)
            {
                float t = gestor.TimeRemaining;
                int minutos = Mathf.FloorToInt(t / 60);
                int segundos = Mathf.FloorToInt(t % 60);
                textoTemporizador.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }

            // Puntuación
            ActualizarRankingHUD();
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
                    foreach (var entry in ranking)
                    {
                        string uid = entry.Key;
                        string nombre = uid;
                        int lvl = 1;

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
