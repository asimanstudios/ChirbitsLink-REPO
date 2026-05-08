namespace ChibitsLink.main.cs.viewmodel;

/// <summary>
/// Modelo de presentación para un jugador individual en la sala de lobby.
/// Evita exponer el modelo de dominio directamente al binding de la vista.
/// </summary>
public class PlayerItem : BaseViewModel
{
    private string _name = string.Empty;
    private string _characterImage = "char_placeholder";
    private bool _isReady = false;
    private int _level = 1;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string CharacterImage { get => _characterImage; set => SetProperty(ref _characterImage, value); }
    public bool IsReady { get => _isReady; set => SetProperty(ref _isReady, value); }
    public int Level { get => _level; set => SetProperty(ref _level, value); }
    public string LevelDisplay => $"LVL. {Level}";
}
