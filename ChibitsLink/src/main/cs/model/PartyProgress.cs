namespace ChibitsLink.main.cs.model;

using System;
using System.Collections.Generic;
/*
 * progreso de las partis como ganadores, puntajes , juegos etc - esto se alamcenara y se debe hacer una vista de historial
 */
public class PartyProgress
{
    public string Id { get; set; } = string.Empty;
    public string PartyId { get; set; } = string.Empty;
    public int? WinnerId { get; set; }
    public Dictionary<int, int> PlayerScores { get; set; } = new Dictionary<int, int>();
    public DateTime CompletedAt { get; set; }
}