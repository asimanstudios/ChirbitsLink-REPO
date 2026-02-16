namespace ChibitsLink.main.cs.model;
/*
 * entidad juego con sus repectivos datos habran:
 * - juego futboll simple
 * - juego saltos con soplido a micro (sendsor)
 * - un juego que use acelerometro para controlar movimiento
 * - juego cocina  basico con movimiento de mando comun con eje de movimiento , salto e itnteractuar
 */
public enum GameType
{
    Soccer,
    Jump,
    Accelerometer,
    Kitchen
}

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GameType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}