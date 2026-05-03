namespace ChibitsLink.main.cs.model;

using System.Collections.Generic;
/*
 * datos de usuario como nombre, username, contraseña (debe manejar un hash), personaje (id del mismo enviara peticiones para q el juego en unity le setee el prefab + otros dato relevantes como nivel de ussuario e historial de partidas del mismo
 */
using Plugin.CloudFirestore.Attributes;

public class User
{
    [Id]
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string SelectedCharacterId { get; set; } = "barbarian";
    
    [MapTo("Level")]
    public int Level { get; set; } = 1;
    
    [MapTo("Experience")]
    public int Experience { get; set; } = 0;
    
    public List<string> GameHistory { get; set; } = new List<string>();
    public List<string> XpClaimedParties { get; set; } = new List<string>();
}
