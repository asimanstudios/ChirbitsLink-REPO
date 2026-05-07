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
            if (_gameManager == null) return "Unknown";
            return _gameManager.GetCarrierName();
        }
        
        public int GetAliveCount()
        {
            if (_scoring == null) return 0;
            return _scoring.GetWinners().Count;
        }
        
        public List<GameObject> GetWinners()
        {
            if (_scoring == null) return new List<GameObject>();
            return _scoring.GetWinners();
        }
        
        public List<GameObject> GetEliminationOrder()
        {
            if (_scoring == null) return new List<GameObject>();
            return _scoring.GetEliminationOrder();
        }
        
        public string GetPlayerName(GameObject player)
        {
            if (_scoring == null) return "Unknown";
            return _scoring.GetPlayerName(player);
        }
        
        public BombTagState GetCurrentState()
        {
            if (_gameManager == null) return BombTagState.Preparing;
            return _gameManager.CurrentState;
        }
        
        public float GetRemainingBombTime()
        {
            if (_gameManager == null) return 0f;
            return _gameManager.remainingBombTime;
        }
        
        public int GetCurrentCountdown()
        {
            if (_gameManager == null) return 0;
            return _gameManager.currentCountdown;
        }
        
        public float GetResultTimeRemaining()
        {
            if (_gameManager == null) return 0f;
            return _gameManager.resultTime;
        }
    }
}
