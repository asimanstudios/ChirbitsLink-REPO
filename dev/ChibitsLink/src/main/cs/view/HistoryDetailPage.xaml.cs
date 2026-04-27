using ChibitsLink.main.cs.model;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.repository;
using Microsoft.Maui.Controls.Shapes;

namespace ChibitsLink.main.cs.view;

public partial class HistoryDetailPage : ContentPage
{
    private readonly Party _party;
    private readonly Database _db;
    private List<Character> _allCharacters = new();

    public HistoryDetailPage(Party party, Database db)
    {
        InitializeComponent();
        _party = party;
        _db = db;
        
        LoadDetails();
    }

    private async void LoadDetails()
    {
        try 
        {
            RoomLabel.Text = $"SALA {_party.RoomCode}";
            DateLabel.Text = _party.CreatedAt.ToString("dd MMM yyyy HH:mm").ToUpper();

            // Cargar todos los personajes para tener las imágenes
            _allCharacters = await _db.GetCharacters();

            PopulatePlayers();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading details: {ex.Message}");
        }
    }

    private void PopulatePlayers()
    {
        PlayersList.Children.Clear();

        if (_party.PlayerScores == null || _party.PlayerScores.Count == 0)
        {
            PlayersList.Children.Add(new Label 
            { 
                Text = "No hay datos de jugadores en esta batalla.", 
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Gray 
            });
            return;
        }

        // Ordenar por puntuación para el ranking
        var rankedPlayers = _party.PlayerScores.OrderByDescending(x => x.Value).ToList();
        int rank = 1;

        foreach (var kvp in rankedPlayers)
        {
            string userId = kvp.Key;
            int score = kvp.Value;
            string name = _party.ParticipantNames != null && _party.ParticipantNames.ContainsKey(userId) 
                ? _party.ParticipantNames[userId] : "Jugador";
            string charId = _party.ParticipantCharacters != null && _party.ParticipantCharacters.ContainsKey(userId)
                ? _party.ParticipantCharacters[userId] : "barbarian";

            var character = _allCharacters.FirstOrDefault(c => c.Id == charId);
            string imageUrl = character?.ImageUrl ?? "char_placeholder.png";

            PlayersList.Children.Add(CreatePlayerCard(rank, name, score, imageUrl));
            rank++;
        }
    }

    private View CreatePlayerCard(int rank, string name, int score, string imageUrl)
    {
        var frame = new Border
        {
            Style = (Style)Application.Current.Resources["GlassFrame"],
            Margin = new Thickness(0, 5),
            Padding = new Thickness(15),
            Stroke = rank == 1 ? (Color)Application.Current.Resources["Secondary"] : (Color)Application.Current.Resources["Primary"]
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto }, // Rank / Medal
                new ColumnDefinition { Width = GridLength.Auto }, // Char Image
                new ColumnDefinition { Width = GridLength.Star }, // Name & Stats
                new ColumnDefinition { Width = GridLength.Auto }  // Score
            },
            ColumnSpacing = 15
        };

        // Rank / Medal
        string rankIcon = rank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => $"#{rank}"
        };

        grid.Add(new Label 
        { 
            Text = rankIcon, 
            FontSize = 24, 
            VerticalOptions = LayoutOptions.Center,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current.Resources["TextMain"]
        }, 0);

        // Character Image
        var image = new Image
        {
            Source = imageUrl,
            WidthRequest = 50,
            HeightRequest = 50,
            Aspect = Aspect.AspectFill,
            Clip = new RoundRectangleGeometry { CornerRadius = new CornerRadius(25) },
            BackgroundColor = Color.FromRgba(255, 255, 255, 0.1)
        };
        grid.Add(image, 1);

        // Name & Info
        var nameStack = new VerticalStackLayout { VerticalOptions = LayoutOptions.Center };
        nameStack.Add(new Label 
        { 
            Text = name.ToUpper(), 
            FontAttributes = FontAttributes.Bold, 
            TextColor = (Color)Application.Current.Resources["TextMain"],
            FontSize = 16
        });
        nameStack.Add(new Label 
        { 
            Text = rank == 1 ? "CAMPEÓN DEL REINO" : "PARTICIPANTE", 
            FontSize = 10, 
            TextColor = (Color)Application.Current.Resources["Secondary"],
            CharacterSpacing = 1
        });
        grid.Add(nameStack, 2);

        // Score
        grid.Add(new Label 
        { 
            Text = $"{score} PTS", 
            FontAttributes = FontAttributes.Bold, 
            VerticalOptions = LayoutOptions.Center,
            TextColor = (Color)Application.Current.Resources["TextMain"],
            FontSize = 18
        }, 3);

        frame.Content = grid;
        return frame;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
