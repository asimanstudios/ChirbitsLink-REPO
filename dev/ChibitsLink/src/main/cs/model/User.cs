namespace ChibitsLink.main.cs.model;

using System.Collections.Generic;
/*
 * datos de usuario como nombre, username, contraseña (debe manejar un hash), personaje (id del mismo enviara peticiones para q el juego en unity le setee el prefab + otros dato relevantes como nivel de ussuario e historial de partidas del mismo
 */
public class User
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string SelectedCharacterId { get; set; } = "VALIENTE"; // Default starting character
    public int Level { get; set; } = 1;
    public List<string> GameHistory { get; set; } = new List<string>();
}
