namespace ChibitsLink.main.cs.model;

/// <summary>
/// Representa un minijuego disponible en la plataforma ChirBits.
/// Campos que Unity escribe en Firestore: Id, Name, Description, ImageUrl.
/// El campo Type es metadato interno de la App (seeder/UI) y NO se persiste en Firestore.
/// </summary>
public class Game
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    /// <summary>Categoría interna del minijuego. No se escribe en Firestore desde Unity.</summary>
    public GameType Type { get; set; } = GameType.Soccer;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}