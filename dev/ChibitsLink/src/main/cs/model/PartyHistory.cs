using System;

namespace ChibitsLink.main.cs.model;

public class PartyHistory
{
    public string Id { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public string CharacterId { get; set; } = string.Empty;
    public bool Won { get; set; }
    
    // Campo para guardar puntos finales en el historial si se desea
    public int FinalScore { get; set; }
}
