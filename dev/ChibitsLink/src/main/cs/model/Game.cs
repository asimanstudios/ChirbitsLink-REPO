namespace ChibitsLink.main.cs.model;

/// <summary>
/// Representa un minijuego disponible en la plataforma ChirBits.
/// </summary>
public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GameType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}