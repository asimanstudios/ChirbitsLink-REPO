using Microsoft.Maui.Graphics;

namespace ChibitsLink.main.cs.view;

/// <summary>
/// Modelo de presentación para el ítem de un jugador en la pantalla de detalle del historial.
/// Evita exponer el modelo de dominio directamente al binding de la vista.
/// </summary>
public class HistoryPlayerItem
{
    public string Name { get; set; } = string.Empty;
    public string ScoreDisplay { get; set; } = string.Empty;
    public string LevelDisplay { get; set; } = string.Empty;
    public string CharacterImage { get; set; } = "char_default.png";
    public string RankDisplay { get; set; } = string.Empty;
    public Color RankColor { get; set; } = Colors.White;
}
