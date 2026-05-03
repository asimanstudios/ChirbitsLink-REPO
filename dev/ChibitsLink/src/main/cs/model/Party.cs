using System;
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
    public List<string> PlayerIds { get; set; } = new List<string>();
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 11000;
    public string GameState { get; set; } = "LOBBY"; // LOBBY, VOTING, IN_GAME
    public List<string> ReadyPlayerIds { get; set; } = new List<string>();
    public Dictionary<string, int> Votes { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> PlayerScores { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, string> ParticipantNames { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> ParticipantCharacters { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, int> ParticipantLevels { get; set; } = new Dictionary<string, int>();
    
    // Historial
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> PlayedGames { get; set; } = new List<string>();
}