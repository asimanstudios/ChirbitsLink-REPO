using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chirbits.Core;

namespace ChibiCocina.CoinCollector
{
    public enum GameState
    {
        Preparing,
        Countdown,
        InGame,
        Finished,
        TransitioningToLobby
    }

    public class GestorCoinCollector : BaseMinigameManager
    {
        public static GestorCoinCollector Instancia { get; private set; }

        public GameState estadoActual 
        {
            get
            {
                return currentState switch
                {
                    MinigameState.Preparing => GameState.Preparing,
                    MinigameState.Countdown => GameState.Countdown,
                    MinigameState.InGame => GameState.InGame,
                    MinigameState.Result => GameState.Finished,
                    MinigameState.Ending => GameState.TransitioningToLobby,
                    _ => GameState.Preparing
                };
            }
        }

        public float tiempoRestante { get; private set; }
        public int valorCuentaAtras { get; private set; }
        public float tiempoRegresoRestante { get; private set; }

        private Dictionary<string, int> puntuaciones = new Dictionary<string, int>();

        protected override void Awake()
        {
            base.Awake();
            if (Instancia == null) Instancia = this;
            else Destroy(gameObject);
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
