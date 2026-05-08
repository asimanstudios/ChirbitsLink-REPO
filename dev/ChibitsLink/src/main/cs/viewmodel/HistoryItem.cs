using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.cs.viewmodel;

/// <summary>
/// Modelo de presentación para un ítem de la lista del historial de partidas.
/// Evita problemas de binding con modelos de dominio complejos.
/// </summary>
public class HistoryItem
{
    public Party OriginalParty { get; set; } = null!;
    public string Code { get; set; } = string.Empty;
    public string DateText { get; set; } = string.Empty;
    public string PlayerCount { get; set; } = string.Empty;
}
