namespace ChibitsLink.main.cs.model;

using System.Collections.Generic;
/*
 * datos de usuario como nombre, username, contraseña (debe manejar un hash), personaje (id del mismo enviara peticiones para q el juego en unity le setee el prefab + otros dato relevantes como nivel de ussuario e historial de partidas del mismo
 */
public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string SelectedCharacterId { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> GameHistory { get; set; } = new List<string>();
}