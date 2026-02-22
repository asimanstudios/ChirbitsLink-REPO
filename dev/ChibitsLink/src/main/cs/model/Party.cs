namespace ChibitsLink.main.cs.model;

using System.Collections.Generic;
/*
 * entidad de party estas son las lobbys
 */
public class Party
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public List<string> PlayerIds { get; set; } = new List<string>();
    public int MaxPlayers { get; set; } = 4;
    public int CurrentPlayers { get; set; } = 0;
    public string HostUserId { get; set; } = string.Empty;
    public bool IsGameStarted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
