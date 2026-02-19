using System;

namespace ChibitsLink.main.cs.model;

public class LobbyHistory
{
    public string Id { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string CharacterId { get; set; } = string.Empty;
    public bool Won { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
