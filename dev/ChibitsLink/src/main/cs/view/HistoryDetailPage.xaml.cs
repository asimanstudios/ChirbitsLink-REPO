using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository.interfaces;
using System.Collections.ObjectModel;

namespace ChibitsLink.main.cs.view;

public partial class HistoryDetailPage : ContentPage
{
    private readonly Party _party;
    private readonly IMasterDataRepository _masterRepo;
    private readonly IUserRepository _userRepo;

    public HistoryDetailPage(Party party, IMasterDataRepository masterRepo, IUserRepository userRepo)
    {
        InitializeComponent();
        _party = party ?? throw new ArgumentNullException(nameof(party));
        _masterRepo = masterRepo ?? throw new ArgumentNullException(nameof(masterRepo));
        _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDetailAsync();
    }

    private async Task LoadDetailAsync()
    {
        try
        {
            SetLoadingVisible(true);

            RoomLabel.Text = $"SALA #{_party.RoomCode}";
            DateLabel.Text = _party.CreatedAt.ToLocalTime().ToString("dd MMMM yyyy · HH:mm").ToUpper();

            // 1. Obtener catálogos
            var characters = await _masterRepo.GetCharactersAsync();
            var charMap = characters.ToDictionary(c => c.Id, c => c, StringComparer.OrdinalIgnoreCase);
            
            var games = await _masterRepo.GetGamesAsync();
            var gameMap = games.ToDictionary(g => g.Id, g => g, StringComparer.OrdinalIgnoreCase);

            var playerItems = new List<HistoryPlayerItem>();
            
            // 2. Extraer lista de IDs reales
            var playerIds = _party.PlayerIds ?? new List<string>();
            if (playerIds.Count == 0 && _party.ParticipantNames != null)
            {
                playerIds = _party.ParticipantNames.Keys.ToList();
            }

            var scoresDict = _party.PlayerScores ?? new Dictionary<string, int>();

            // Ordenar IDs por puntuación
            var rankedIds = playerIds.OrderByDescending(id => scoresDict.TryGetValue(id, out var s) ? s : 0).ToList();

            // 3. Buscar a todos los usuarios en la Base de Datos (en paralelo)
            var userTasks = rankedIds.Select(id => _userRepo.GetUserAsync(id));
            var users = await Task.WhenAll(userTasks);

            // 4. Construir las tarjetas cruzando Datos de la BBDD + Puntos de la Partida
            string userId;
            User? dbUser;
            int score;
            int s;
            int rank;
            string name;
            string charId;
            int level;
            string? n;
            string? c;
            int l;
            Character? character;

            for (int i = 0; i < rankedIds.Count; i++)
            {
                userId = rankedIds[i];
                dbUser = users[i]; // El perfil descargado desde Firestore
                
                score = scoresDict.TryGetValue(userId, out s) ? s : 0;
                rank = i + 1;

                // Si se encontró al usuario en la base de datos, usamos sus datos reales
                name = dbUser != null ? dbUser.Username : "JUGADOR";
                charId = dbUser != null ? dbUser.SelectedCharacterId : "";
                level = dbUser != null ? dbUser.Level : 1;

                // Si no se encontró en la BBDD, intentamos sacar lo que se guardó de respaldo en la Party
                if (dbUser == null)
                {
                    if (_party.ParticipantNames != null && _party.ParticipantNames.TryGetValue(userId, out n)) name = n;
                    if (_party.ParticipantCharacters != null && _party.ParticipantCharacters.TryGetValue(userId, out c)) charId = c;
                    if (_party.ParticipantLevels != null && _party.ParticipantLevels.TryGetValue(userId, out l)) level = l;
                }

                charMap.TryGetValue(charId, out character);
                
                playerItems.Add(new HistoryPlayerItem
                {
                    Name = name.ToUpper(),
                    ScoreDisplay = $"{score} PTS",
                    LevelDisplay = $"NIVEL {level}",
                    CharacterImage = character?.ImageUrl ?? "char_placeholder.png",
                    RankDisplay = rank switch { 1 => "🥇", 2 => "🥈", 3 => "🥉", _ => $"#{rank}" },
                    RankColor = rank switch {
                        1 => Color.FromArgb("#FFD700"), // Oro
                        2 => Color.FromArgb("#C0C0C0"), // Plata
                        3 => Color.FromArgb("#CD7F32"), // Bronce
                        _ => Color.FromArgb("#1AFFFFFF") // Normal
                    }
                });
            }

            PlayersListView.ItemsSource = playerItems;
            BuildGamesList(gameMap);
        }
        catch (ChibitsLink.main.cs.exception.DatabaseException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistoryDetailPage] Error Firestore: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("[HistoryDetailPage] Carga cancelada.");
        }
        finally
        {
            SetLoadingVisible(false);
        }
    }

    private void BuildGamesList(Dictionary<string, Game> gameMap)
    {
        GamesFlexList.Children.Clear();
        if (_party.PlayedGames != null)
        {
            Game? gameInfo;
            string displayName;
            Border chip;
            foreach (var gameId in _party.PlayedGames)
            {
                gameMap.TryGetValue(gameId, out gameInfo);
                displayName = gameInfo?.Name ?? gameId.Replace("Minigame_", "").ToUpper();

                chip = new Border
                {
                    Style = (Style)Application.Current!.Resources["GlassFrame"],
                    Margin = new Thickness(4),
                    Padding = new Thickness(12, 6),
                    Stroke = (Color)Application.Current.Resources["Secondary"],
                    Content = new Label { Text = displayName, FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Colors.White }
                };
                GamesFlexList.Children.Add(chip);
            }
        }
    }

    private void SetLoadingVisible(bool loading)
    {
        LoadingIndicator.IsRunning = loading;
        LoadingIndicator.IsVisible = loading;
        
        if (!loading)
        {
            var players = PlayersListView.ItemsSource as IList<HistoryPlayerItem>;
            if (players == null || players.Count == 0)
            {
                EmptyStateView.IsVisible = true;
                PlayersListView.IsVisible = false;
            }
            else
            {
                EmptyStateView.IsVisible = false;
                PlayersListView.IsVisible = true;
            }
        }
        else
        {
            EmptyStateView.IsVisible = false;
            PlayersListView.IsVisible = false;
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e) => await Navigation.PopModalAsync();
}
