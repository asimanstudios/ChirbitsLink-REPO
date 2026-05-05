using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;

namespace ChibiCocina.CoinCollector
{
    public class UICoinCollector : MonoBehaviour
    {
        [Header("HUD")]
        public TextMeshProUGUI textoTemporizador;
        public TextMeshProUGUI textoPuntuacionGlobal; // Cambiado para mostrar todos o el local
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
            bool hasManagerInstance = GestorCoinCollector.Instancia != null;
            if (hasManagerInstance)
            {
                ActualizarHUD();
                ActualizarEstadoVisual();
            }
        }

        private void ActualizarHUD()
        {
            var gestor = GestorCoinCollector.Instancia;

            // Timer
            if (textoTemporizador != null)
            {
                float t = gestor.tiempoRestante;
                int minutos = Mathf.FloorToInt(t / 60);
                int segundos = Mathf.FloorToInt(t % 60);
                textoTemporizador.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }

            // Puntuación
            ActualizarRankingHUD();
        }

        private string _cachedRankingText = "";
        private int _cachedScoreHash = 0;

        private void ActualizarRankingHUD()
        {
            bool canUpdateRanking = textoPuntuacionGlobal != null && GestorCoinCollector.Instancia != null;
            if (canUpdateRanking)
            {
                var ranking = GestorCoinCollector.Instancia.ObtenerRanking();
                
                // OPTIMIZACIÓN: Solo reconstruir el string si el hash de puntuaciones ha cambiado
                int currentHash = 0;
                foreach (var r in ranking) currentHash ^= r.Key.GetHashCode() ^ r.Value.GetHashCode();

                bool canUseCachedText = currentHash == _cachedScoreHash && !string.IsNullOrEmpty(_cachedRankingText);
                if (canUseCachedText)
                {
                    textoPuntuacionGlobal.text = _cachedRankingText;
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

                    _cachedRankingText = sb.ToString();
                    textoPuntuacionGlobal.text = _cachedRankingText;
                }
            }
        }

        private void ActualizarEstadoVisual()
        {
            var gestor = GestorCoinCollector.Instancia;
            GameState estado = gestor.estadoActual;

            // Cuenta Atrás
            if (estado == GameState.Countdown)
            {
                if (textoCuentaAtras != null)
                {
                    textoCuentaAtras.gameObject.SetActive(true);
                    textoCuentaAtras.text = gestor.valorCuentaAtras.ToString();
                }
            }
            else
            {
                if (textoCuentaAtras != null) textoCuentaAtras.gameObject.SetActive(false);
            }

            // HUD y Resultados
            if (estado == GameState.InGame)
            {
                if (panelHUD != null) panelHUD.SetActive(true);
                if (panelResultados != null) panelResultados.SetActive(false);
            }

            bool isResultState = estado == GameState.Finished || estado == GameState.TransitioningToLobby;
            if (isResultState)
            {
                if (panelHUD != null) panelHUD.SetActive(false);
                if (panelResultados != null && !panelResultados.activeSelf)
                {
                    MostrarResultados();
                }
                
                // Si estamos en transición, forzamos recarga de puntos en los resultados 
                // para que se vea el contador de tiempo de regreso
                if (estado == GameState.TransitioningToLobby)
                {
                    MostrarResultados();
                }
            }
        }

        public void MostrarResultados()
        {
            if (panelResultados != null)
            {
                panelResultados.SetActive(true);

                var ranking = GestorCoinCollector.Instancia.ObtenerRanking();
                StringBuilder sb = new StringBuilder();
                
                var gestor = GestorCoinCollector.Instancia;
                if (gestor.estadoActual == GameState.TransitioningToLobby)
                {
                    sb.AppendLine($"<color=orange>Volviendo al menú en... {gestor.tiempoRegresoRestante:0}</color>\n");
                }

                sb.AppendLine("<size=120%>RANKING FINAL</size>\n");

                for (int i = 0; i < ranking.Count; i++)
                {
                    string prefijo = (i == 0) ? "👑 " : (i + 1) + ". ";
                    string uid = ranking[i].Key;
                    string nombre = uid;
                    int lvl = 1;
                    
                    if (ChibitsLink.GameSide.PlayerManager.Instance != null)
                    {
                        nombre = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerName(uid);
                        lvl = ChibitsLink.GameSide.PlayerManager.Instance.GetPlayerLevel(uid);
                    }

                    sb.AppendLine($"{prefijo}[Lvl {lvl}] {nombre}: {ranking[i].Value} monedas");
                }

                if (textoRanking != null) textoRanking.text = sb.ToString();
            }
        }
    }
}
