using System;
using UnityEngine;

namespace Chirbits.Core.Networking
{
    public class NetworkMessageEventArgs : EventArgs
    {
        public string UserId { get; set; }
        public string RawMessage { get; set; }
        public string Command { get; set; }
        public string[] Payload { get; set; }
    }

    public static class NetworkingEvents
    {
        public static event Action<string, string> OnClientConnected;
        public static event Action<string> OnClientDisconnected;
        public static event Action<NetworkMessageEventArgs> OnMessageReceived;

        public static void RaiseConnected(string userId, string endpoint) => OnClientConnected?.Invoke(userId, endpoint);
        public static void RaiseDisconnected(string userId) => OnClientDisconnected?.Invoke(userId);
        public static void RaiseMessageReceived(NetworkMessageEventArgs args) => OnMessageReceived?.Invoke(args);
    }
}
