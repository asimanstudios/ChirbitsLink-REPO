using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository;
using Microsoft.Maui.Controls.Shapes;

namespace ChibitsLink.main.cs.view;

/// <summary>
/// Pantalla de detalle de una partida cerrada. Muestra ranking de jugadores
/// con personaje, nombre real y puntuación obtenida, todo desde Firestore.
/// No contiene datos hardcodeados: si un campo está vacío en la BBDD, se omite con gracia.
/// </summary>
public partial class HistoryDetailPage : ContentPage
{
    private readonly Party  _party;
    private readonly Database _db;

    public HistoryDetailPage(Party party, Database db)
    {
        InitializeComponent();
        _party = party ?? throw new ArgumentNullException(nameof(party));
        _db    = db    ?? throw new ArgumentNullException(nameof(db));
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDetailAsync();
    }

    // ── Data loading ──────────────────────────────────────────────────────────

    private async Task LoadDetailAsync()
    {
        try
        {
            SetLoadingVisible(true);

            RoomLabel.Text = string.IsNullOrEmpty(_party.RoomCode)
                ? "SALA DESCONOCIDA"
                : $"SALA #{_party.RoomCode}";

            DateLabel.Text = _party.CreatedAt == default
                ? "Fecha no disponible"
                : _party.CreatedAt.ToLocalTime().ToString("dd MMM yyyy  HH:mm").ToUpper();

            // Juegos jugados
            GamesLabel.Text = (_party.PlayedGames == null || _party.PlayedGames.Count == 0)
                ? "Sin registro de juegos"
                : string.Join("  ·  ", _party.PlayedGames);

            // Cargar personajes de Firestore (una vez) para las imágenes
            var characters = await _db.GetCharacters();
            var charMap    = characters.ToDictionary(c => c.Id, c => c, StringComparer.OrdinalIgnoreCase);

            BuildPlayerRanking(charMap);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryDetailPage] LoadDetailAsync error: {ex.Message}");
            await DisplayAlert("Error", "No se pudieron cargar los detalles de esta batalla.", "Cerrar");
        }
        finally
        {
            SetLoadingVisible(false);
        }
    }

    // ── UI Building ───────────────────────────────────────────────────────────

    private void BuildPlayerRanking(Dictionary<string, Character> charMap)
    {
        PlayersList.Children.Clear();

        if (_party.PlayerScores == null || _party.PlayerScores.Count == 0)
        {
            PlayersList.Children.Add(BuildEmptyState());
            return;
        }

        // Ordenar de mayor a menor puntuación
        var ranked = _party.PlayerScores
            .OrderByDescending(kvp => kvp.Value)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
        {
            string userId = ranked[i].Key;
            int    score  = ranked[i].Value;
            int    rank   = i + 1;

            // Nombre: desde ParticipantNames. Si no existe, mostrar UID truncado (nunca hardcode "Jugador")
            string displayName = (_party.ParticipantNames != null &&
                                  _party.ParticipantNames.TryGetValue(userId, out var n) &&
                                  !string.IsNullOrWhiteSpace(n))
                ? n
                : $"[{userId[..Math.Min(6, userId.Length)]}...]";

            // Personaje: desde ParticipantCharacters
            string charId = (_party.ParticipantCharacters != null &&
                             _party.ParticipantCharacters.TryGetValue(userId, out var c) &&
                             !string.IsNullOrWhiteSpace(c))
                ? c : string.Empty;

            charMap.TryGetValue(charId, out var character);
            string imageSource = (!string.IsNullOrEmpty(character?.ImageUrl))
                ? character.ImageUrl
                : "char_default.png";   // asset de fallback genérico en el proyecto

            PlayersList.Children.Add(BuildPlayerCard(rank, displayName, score, imageSource));
        }
    }

    private View BuildPlayerCard(int rank, string name, int score, string imageSource)
    {
        string medal = rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{rank}" };
        bool   isWinner = rank == 1;

        var border = new Border
        {
            Style   = (Style)Application.Current!.Resources["GlassFrame"],
            Margin  = new Thickness(0, 6),
            Padding = new Thickness(15),
            Stroke  = isWinner
                ? (Color)Application.Current.Resources["Secondary"]
                : (Color)Application.Current.Resources["Primary"]
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = 40 },               // medal
                new ColumnDefinition { Width = 56 },               // avatar
                new ColumnDefinition { Width = GridLength.Star },   // name + tag
                new ColumnDefinition { Width = GridLength.Auto }    // score
            },
            ColumnSpacing = 12,
            VerticalOptions = LayoutOptions.Center
        };

        // — Medal
        grid.Add(new Label
        {
            Text            = medal,
            FontSize        = 22,
            VerticalOptions = LayoutOptions.Center
        }, 0);

        // — Avatar
        var avatar = new Image
        {
            Source          = imageSource,
            WidthRequest    = 50,
            HeightRequest   = 50,
            Aspect          = Aspect.AspectFill,
            Clip            = new RoundRectangleGeometry { CornerRadius = new CornerRadius(25) }
        };
        grid.Add(avatar, 1);

        // — Name + tag
        var nameStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center };
        nameStack.Add(new Label
        {
            Text            = name.ToUpperInvariant(),
            FontAttributes  = FontAttributes.Bold,
            FontSize        = 15,
            TextColor       = (Color)Application.Current.Resources["TextMain"]
        });
        nameStack.Add(new Label
        {
            Text          = isWinner ? "CAMPEÓN" : "PARTICIPANTE",
            FontSize      = 10,
            CharacterSpacing = 1.5,
            TextColor     = isWinner
                ? (Color)Application.Current.Resources["Secondary"]
                : (Color)Application.Current.Resources["TextLight"]
        });
        grid.Add(nameStack, 2);

        // — Score
        grid.Add(new Label
        {
            Text            = $"{score} pts",
            FontAttributes  = FontAttributes.Bold,
            FontSize        = 17,
            VerticalOptions = LayoutOptions.Center,
            TextColor       = (Color)Application.Current.Resources["TextMain"]
        }, 3);

        border.Content = grid;
        return border;
    }

    private static View BuildEmptyState() =>
        new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions   = LayoutOptions.Center,
            Spacing           = 8,
            Children =
            {
                new Label { Text = "⚔️", FontSize = 40, HorizontalOptions = LayoutOptions.Center },
                new Label
                {
                    Text              = "No hay puntuaciones registradas para esta batalla.",
                    FontSize          = 13,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
        };

    private void SetLoadingVisible(bool loading)
    {
        LoadingIndicator.IsRunning = loading;
        LoadingIndicator.IsVisible = loading;
        ContentScrollView.IsVisible = !loading;
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
