using UnityEngine;

namespace ChibitsLink.Core
{
    public class GameTimer : MonoBehaviour
    {
        [Header("Timer Configuration")]
        public float preparationTime = 5f;
        public float gameTime = 300f;
        
        private float _remainingTime;
        private bool _isRunning;
        
        public System.Action<float> OnTimeUpdated;
        
        public void Initialize()
        {
            _remainingTime = preparationTime;
            _isRunning = false;
        }
        
        public void StartTimer(float duration)
        {
            _remainingTime = duration;
            _isRunning = true;
        }
        
        public void StopTimer()
        {
            _isRunning = false;
        }
        
        public void UpdateTimer()
        {
            if (_isRunning)
            {
                _remainingTime -= Time.deltaTime;
                OnTimeUpdated?.Invoke(_remainingTime);
            }
        }
        
        public bool IsTimeExpired()
        {
            return _remainingTime <= 0f;
        }
        
        public float GetRemainingTime()
        {
            return _remainingTime;
        }
        
        public void SetPreparationTime()
        {
            _remainingTime = preparationTime;
        }
        
        public void SetGameTime()
        {
            _remainingTime = gameTime;
        }
    }
}
