using System.Collections.Generic;

namespace ChibitsLink.main.cs.model;

/// <summary>
/// Representa una sala de juego (lobby) activa o pasada en ChirBits.
/// Contiene el código de sala y los jugadores que participaron.
/// </summary>
public class Party
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public List<int> PlayerIds { get; set; } = new List<int>();
}