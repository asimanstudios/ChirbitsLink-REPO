using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace ChibitsLink.UI
{
    /// <summary>
    /// Manages on-screen notifications for player connections/disconnections.
    /// </summary>
    public class LobbyNotifications : MonoBehaviour
    {
        public TextMeshProUGUI notificationText;
        public float displayTime = 3f;
        
        private Queue<string> _messageQueue = new Queue<string>();
        private bool _isShowing = false;

        void Awake()
        {
            if (notificationText != null)
                notificationText.text = "";
        }

        public void ShowNotification(string message)
        {
            _messageQueue.Enqueue(message);
            if (!_isShowing)
            {
                StartCoroutine(ProcessQueue());
            }
        }

        private IEnumerator ProcessQueue()
        {
            _isShowing = true;
            while (_messageQueue.Count > 0)
            {
                string msg = _messageQueue.Dequeue();
                if (notificationText != null)
                {
                    notificationText.text = msg;
                    yield return new WaitForSeconds(displayTime);
                    notificationText.text = "";
                }
                yield return new WaitForSeconds(0.5f);
            }
            _isShowing = false;
        }
    }
}
