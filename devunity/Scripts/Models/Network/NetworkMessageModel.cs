using System;
using System.Text;

namespace ChibiCocina.Models
{
    public class NetworkMessageModel
    {
        public string Type { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public MessagePriority Priority { get; set; }
        public bool RequiresResponse { get; set; }
        
        public NetworkMessageModel()
        {
            Timestamp = DateTime.Now;
            Priority = MessagePriority.Normal;
        }
        
        public string ToJson()
        {
            return $@"{{""type"":""{Type}"",""userId"":""{UserId}"",""content"":""{Content}"",""timestamp"":""{Timestamp:yyyy-MM-dd HH:mm:ss}""}}";
        }
        
        public static NetworkMessageModel FromJson(string json)
        {
            // Implementación básica de parsing JSON
            var message = new NetworkMessageModel();
            
            if (json.Contains("\"type\":"))
            {
                var typeStart = json.IndexOf("\"type\":\"") + 8;
                var typeEnd = json.IndexOf("\"", typeStart);
                message.Type = json.Substring(typeStart, typeEnd - typeStart);
            }
            
            if (json.Contains("\"userId\":"))
            {
                var userIdStart = json.IndexOf("\"userId\":\"") + 10;
                var userIdEnd = json.IndexOf("\"", userIdStart);
                message.UserId = json.Substring(userIdStart, userIdEnd - userIdStart);
            }
            
            if (json.Contains("\"content\":"))
            {
                var contentStart = json.IndexOf("\"content\":\"") + 11;
                var contentEnd = json.IndexOf("\"", contentStart);
                message.Content = json.Substring(contentStart, contentEnd - contentStart);
            }
            
            return message;
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(UserId);
        }
    }
    
    public class ControllerInputMessage
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string State { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public string UserId { get; set; }
        public string Sensor { get; set; }
        public float Value { get; set; }
        public DateTime Timestamp { get; set; }
        
        public ControllerInputMessage()
        {
            Timestamp = DateTime.Now;
        }
        
        public bool IsButtonInput()
        {
            return !string.IsNullOrEmpty(Type) && (Type == "button" || Type == "touch");
        }
        
        public bool IsJoystickInput()
        {
            return !string.IsNullOrEmpty(Type) && Type == "joystick";
        }
        
        public bool IsSensorInput()
        {
            return !string.IsNullOrEmpty(Sensor);
        }
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Type);
        }
    }
    
    public enum MessagePriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
    
    public enum MessageType
    {
        Unknown,
        Connection,
        Disconnection,
        ControllerInput,
        Sync,
        Ready,
        Vote,
        GameState,
        Chat,
        System
    }
}
