using UnityEngine;
using System.Collections.Generic;

namespace ChibitsLink.Minigames.BombTag
{
    public class BombTagUI : MonoBehaviour
    {
        private BombTagScoring _scoring;
        private BombTagGameManager _gameManager;
        
        public void Initialize(BombTagScoring scoring, BombTagGameManager gameManager)
        {
            _scoring = scoring;
            _gameManager = gameManager;
        }
        
        public string GetCarrierName()
        {
            string name = "Unknown";
            if (_gameManager != null)
            {
                name = _gameManager.GetCarrierName();
            }
            return name;
        }
        
        public int GetAliveCount()
        {
            int count = 0;
            if (_scoring != null)
            {
                count = _scoring.GetWinners().Count;
            }
            return count;
        }
        
        public List<GameObject> GetWinners()
        {
            List<GameObject> winners = null;
            if (_scoring != null)
            {
                winners = _scoring.GetWinners();
            }
            if (winners == null)
            {
                winners = new List<GameObject>();
            }
            return winners;
        }
        
        public List<GameObject> GetEliminationOrder()
        {
            List<GameObject> order = null;
            if (_scoring != null)
            {
                order = _scoring.GetEliminationOrder();
            }
            if (order == null)
            {
                order = new List<GameObject>();
            }
            return order;
        }
        
        public string GetPlayerName(GameObject player)
        {
            string name = "Unknown";
            if (_scoring != null)
            {
                name = _scoring.GetPlayerName(player);
            }
            return name;
        }
        
        public BombTagState GetCurrentState()
        {
            BombTagState state = BombTagState.Preparing;
            if (_gameManager != null)
            {
                state = _gameManager.CurrentState;
            }
            return state;
        }
        
        public float GetRemainingBombTime()
        {
            float time = 0f;
            if (_gameManager != null)
            {
                time = _gameManager.remainingBombTime;
            }
            return time;
        }
        
        public int GetCurrentCountdown()
        {
            int countdown = 0;
            if (_gameManager != null)
            {
                countdown = _gameManager.currentCountdown;
            }
            return countdown;
        }
        
        public float GetResultTimeRemaining()
        {
            float time = 0f;
            if (_gameManager != null)
            {
                time = _gameManager.resultTime;
            }
            return time;
        }
    }
}
