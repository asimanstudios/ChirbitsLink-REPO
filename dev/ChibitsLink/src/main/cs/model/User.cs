using System.Collections.Generic;
using Plugin.CloudFirestore.Attributes;

namespace ChibitsLink.main.cs.model;

/// <summary>
/// Datos de usuario: nombre, username, personaje seleccionado, nivel y historial de partidas.
/// La contraseña se gestiona exclusivamente a través de Firebase Auth (nunca se almacena en texto plano).
/// </summary>
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
