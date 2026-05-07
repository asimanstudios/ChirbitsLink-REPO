using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chirbits.Core;

namespace ChibitsLink.Minigames.CoinCollector
{
    /// <summary>
    /// Estados específicos del minijuego CoinCollector.
    /// Extiende los estados base del minijuego.
    /// </summary>
    public enum CoinCollectorState
    {
        /// <summary>Fase de preparación inicial</summary>
        Preparing,
        /// <summary>Fase de cuenta regresiva</summary>
        Countdown,
        /// <summary>Fase de juego activo</summary>
        InGame,
        /// <summary>Fase finalizada</summary>
        Finished,
        /// <summary>Fase de transición al lobby</summary>
        TransitioningToLobby
    }

    /// <summary>
    /// Gestor del minijuego Coin Collector.
    /// Hereda de BaseMinigameManager e implementa lógica específica.
    /// </summary>
    /// <remarks>
    /// Maneja la recolección de monedas y puntuación.
    /// Implementa temporizador y estados específicos.
    /// Proporciona Singleton para acceso global.
    /// </remarks>
    public class CoinCollectorGameManager : BaseMinigameManager
    {
        /// <summary>Instancia global del gestor (patrón Singleton)</summary>
        public static CoinCollectorGameManager Instance { get; private set; }

        /// <summary>
        /// Estado actual del minijuego específico.
        /// Convierte del estado base al estado específico.
        /// </summary>
        public CoinCollectorState CurrentState 
        {
            get
            {
                return currentState switch
                {
                    MinigameState.Preparing => CoinCollectorState.Preparing,
                    MinigameState.Countdown => CoinCollectorState.Countdown,
                    MinigameState.InGame => CoinCollectorState.InGame,
                    MinigameState.Result => CoinCollectorState.Finished,
                    MinigameState.Ending => CoinCollectorState.TransitioningToLobby,
                    _ => CoinCollectorState.Preparing
                };
            }
        }

        /// <summary>Tiempo restante del juego</summary>
        public float remainingTime { get; private set; }
        /// <summary>Valor actual de la cuenta regresiva</summary>
        public int countdownValue { get; private set; }
        /// <summary>Tiempo restante para regresar al lobby</summary>
        public float returnTimeRemaining { get; private set; }

        /// <summary>Puntuaciones de jugadores por ID</summary>
        private Dictionary<string, int> _scores = new Dictionary<string, int>();

        /// <summary>
        /// Inicialización del gestor del minijuego.
        /// Establece el patrón Singleton y llama a la base.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (Instance == null) 
            {
                Instance = this;
            }
            else 
            {
                Destroy(gameObject);
            }
        }

        protected override void OnGamePreparing()
        {
            puntuaciones.Clear();
            tiempoRestante = 60f; // 1 minuto de partida por defecto
        }

        protected override void OnCountdownTick(int tick)
        {
            valorCuentaAtras = tick;
        }

        protected override void OnGameStarted()
        {
            Debug.Log("[CoinCollector] ¡A por el oro!");
        }

        protected override IEnumerator WaitUntilGameEnds()
        {
            while (tiempoRestante > 0)
            {
                tiempoRestante -= Time.deltaTime;
                yield return null;
            }
        }

        protected override void OnGameResults()
        {
            tiempoRegresoRestante = resultTime;
        }

        public List<KeyValuePair<string, int>> ObtenerRanking()
        {
            return puntuaciones.OrderByDescending(x => x.Value).ToList();
        }

        public List<GameObject> GetVivos() => players;

        public void SumarMoneda(string userId, int puntos = 1)
        {
            if (puntuaciones.ContainsKey(userId)) puntuaciones[userId] += puntos;
            else puntuaciones[userId] = puntos;
            
            ReportScore(userId, puntos);
        }

        public void RegistrarMonedaRecogida(string userId, int puntos) => SumarMoneda(userId, puntos);
    }
}
