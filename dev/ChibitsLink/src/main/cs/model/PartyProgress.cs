using System;
using System.Collections.Generic;

namespace ChibitsLink.main.cs.model;

/// <summary>
/// Registra el progreso de una partida concreta: puntuaciones, ganador y momento de finalización.
/// </summary>
public class PartyProgress
{
    public string Id { get; set; } = string.Empty;
    public string PartyId { get; set; } = string.Empty;
    public int? WinnerId { get; set; }
    public Dictionary<int, int> PlayerScores { get; set; } = new Dictionary<int, int>();
    public DateTime CompletedAt { get; set; }
}