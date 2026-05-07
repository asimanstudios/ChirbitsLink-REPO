using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chirbits.Core;

namespace ChibitsLink.Minigames.CoinCollector
{
    public enum CoinCollectorState
    {
        Preparing,
        Countdown,
        InGame,
        Finished,
        TransitioningToLobby
    }

    public class CoinCollectorGameManager : BaseMinigameManager
    {
        public static CoinCollectorGameManager Instance { get; private set; }

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

        public float remainingTime { get; private set; }
        public int countdownValue { get; private set; }
        public float returnTimeRemaining { get; private set; }

        private Dictionary<string, int> _scores = new Dictionary<string, int>();

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
