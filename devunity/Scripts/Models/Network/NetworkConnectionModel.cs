using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ChibiCocina.Models
{
    public class NetworkConnectionModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string CharacterId { get; set; }
        public int Level { get; set; }
        public string EndPoint { get; set; }
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }
        
        public NetworkConnectionModel()
        {
            ConnectedAt = DateTime.Now;
            LastActivity = DateTime.Now;
            IsActive = true;
        }
        
        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }
        
        public bool IsExpired(TimeSpan timeout)
        {
            return DateTime.Now - LastActivity > timeout;
        }
    }
    
    public class NetworkSessionModel
    {
        public string RoomCode { get; set; }
        public Dictionary<string, NetworkConnectionModel> ActiveConnections { get; set; }
        public Dictionary<string, string> SessionNames { get; set; }
        public Dictionary<string, string> SessionCharacters { get; set; }
        public Dictionary<string, int> SessionLevels { get; set; }
        public List<string> ActivePlayerIds { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public NetworkSessionModel()
        {
            ActiveConnections = new Dictionary<string, NetworkConnectionModel>();
            SessionNames = new Dictionary<string, string>();
            SessionCharacters = new Dictionary<string, string>();
            SessionLevels = new Dictionary<string, int>();
            ActivePlayerIds = new List<string>();
            CreatedAt = DateTime.Now;
        }
        
        public void AddConnection(NetworkConnectionModel connection)
        {
            ActiveConnections[connection.UserId] = connection;
            SessionNames[connection.UserId] = connection.Name;
            SessionCharacters[connection.UserId] = connection.CharacterId;
            SessionLevels[connection.UserId] = connection.Level;
            
            if (!ActivePlayerIds.Contains(connection.UserId))
            {
                ActivePlayerIds.Add(connection.UserId);
            }
        }
        
        public void RemoveConnection(string userId)
        {
            if (ActiveConnections.ContainsKey(userId))
            {
                ActiveConnections.Remove(userId);
            }
            
            ActivePlayerIds.Remove(userId);
        }
        
        public NetworkConnectionModel GetConnection(string userId)
        {
            return ActiveConnections.ContainsKey(userId) ? ActiveConnections[userId] : null;
        }
        
        public int GetActivePlayerCount()
        {
            return ActivePlayerIds.Count;
        }
    }
}
