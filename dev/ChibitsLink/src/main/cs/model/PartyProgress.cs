using System;
using System.Collections.Generic;

namespace ChibitsLink.main.cs.model;

/// <summary>
/// Registra el progreso de una partida concreta: puntuaciones, ganador y momento de finalización.
/// Las claves de PlayerScores son UIDs de usuario (string), igual que en Firestore.
/// </summary>
public class PartyProgress
{
    public string Id { get; set; } = string.Empty;
    public string PartyId { get; set; } = string.Empty;
    public string? WinnerId { get; set; }
    public Dictionary<string, int> PlayerScores { get; set; } = new Dictionary<string, int>();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}