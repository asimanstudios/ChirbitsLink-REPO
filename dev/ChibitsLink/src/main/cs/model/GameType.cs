namespace ChibitsLink.main.cs.model;

/// <summary>
/// Tipos de minijuego disponibles en la plataforma ChirBits.
/// </summary>
public enum GameType
{
    /// <summary>Juego de fútbol simple.</summary>
    Soccer,

    /// <summary>Juego de saltos usando el micrófono como sensor de soplido.</summary>
    Jump,

    /// <summary>Juego controlado mediante el acelerómetro del dispositivo.</summary>
    Accelerometer,

    /// <summary>Juego de cocina con controles de movimiento estándar.</summary>
    Kitchen
}
