// ╔══════════════════════════════════════════════════════════════════════════════╗
// ║  ⚠️  CLASE OBSOLETA — No usar en código nuevo.                              ║
// ║  Esta clase ha sido completamente reemplazada por los repositorios           ║
// ║  modulares: LobbyRepository, UserRepository, MasterDataRepository.           ║
// ║  Puede eliminarse del proyecto de forma segura.                              ║
// ╚══════════════════════════════════════════════════════════════════════════════╝
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using ChibitsLink.main.cs.exception;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.repository;

/// <inheritdoc cref="LobbyRepository"/>
[Obsolete("Usar LobbyRepository, UserRepository y MasterDataRepository en su lugar.")]
public class Database
{
    // ── Constantes de colecciones ────────────────────────────────────────────
    private const string ColUsers      = "users";
    private const string ColParties    = "parties";
    private const string ColGames      = "games";
    private const string ColCharacters = "characters";

    // ── Estados de sala (sincronizados con Unity GameState.cs) ───────────────
    private const string StateOpen   = "LOBBY";
    private const string StateClosed = "CLOSED";

    private readonly IFirestore _firestore;

    public Database(FirebaseConnection connection)
    {
        _firestore = connection?.Firestore
            ?? throw new ArgumentNullException(nameof(connection));
    }

    // ── Listener en tiempo real ───────────────────────────────────────────────

    /// <summary>
    /// Suscribe un listener a cambios en tiempo real de un documento.
    /// El caller es responsable de llamar Dispose() cuando ya no lo necesite.
    /// </summary>
    public IDisposable ListenAsync<T>(string collection, string documentId, Action<T?> onChanged)
        where T : class
    {
        return _firestore
            .Collection(collection)
            .Document(documentId)
            .AddSnapshotListener((snapshot, error) =>
            {
                if (error != null)
                {
                    Log($"ListenAsync error ({collection}/{documentId}): {error.Message}");
                    return;
                }
                onChanged(snapshot is { Exists: true } ? snapshot.ToObject<T>() : null);
            });
    }

    // ── CRUD genérico ─────────────────────────────────────────────────────────

    public async Task StoreAsync<T>(string collection, string documentId, T data)
        where T : class
    {
        try
        {
            await _firestore.Collection(collection).Document(documentId).SetAsync(data);
        }
        catch (ArgumentException ex)
        {
            throw new DatabaseException($"Invalid argument storing to {collection}/{documentId}", ex);
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Failed to store {collection}/{documentId}", ex);
        }
    }

    public async Task<T?> GetAsync<T>(string collection, string documentId)
        where T : class
    {
        try
        {
            var snapshot = await _firestore.Collection(collection).Document(documentId).GetAsync();
            return snapshot.Exists ? snapshot.ToObject<T>() : null;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Failed to get {collection}/{documentId}", ex);
        }
    }

    public async Task<List<T>> ListAsync<T>(string collection)
        where T : class
    {
        try
        {
            var snapshot = await _firestore.Collection(collection).GetAsync();
            return snapshot.Documents
                .Select(d => d.ToObject<T>())
                .Where(x => x != null)
                .Cast<T>()
                .ToList();
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Failed to list {collection}", ex);
        }
    }

    /// <summary>
    /// Inicializa la colección de personajes con datos por defecto si está vacía.
    /// </summary>
    public async Task InitializeCharactersAsync()
    {
        try
        {
            var existing = await ListAsync<Character>(ColCharacters);
            if (existing.Count > 0) return;

            var defaultCharacters = new List<Character>
            {
                new Character { Id = "VALIENTE",    Name = "Valiente",    Description = "Guerrero audaz.", ImageUrl = "char_placeholder" },
                new Character { Id = "MAGO",        Name = "Mago",        Description = "Maestro arcano.", ImageUrl = "char_placeholder" },
                new Character { Id = "EXPLORADOR",  Name = "Explorador",  Description = "Sigiloso.",       ImageUrl = "char_placeholder" },
                new Character { Id = "CURADORA",    Name = "Curadora",    Description = "Apoyo.",          ImageUrl = "char_placeholder" },
                new Character { Id = "TANQUE",      Name = "Tanque",      Description = "Defensa.",        ImageUrl = "char_placeholder" }
            };

            foreach (var c in defaultCharacters)
            {
                await StoreAsync(ColCharacters, c.Id, c);
            }
        }
        catch (Exception ex)
        {
            Log($"Error en InitializeCharactersAsync: {ex.Message}");
        }
    }

    public async Task DeleteAsync(string collection, string documentId)
    {
        try
        {
            await _firestore.Collection(collection).Document(documentId).DeleteAsync();
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Failed to delete {collection}/{documentId}", ex);
        }
    }

    // ── Usuarios ──────────────────────────────────────────────────────────────

    public async Task SaveUser(User user)
        => await StoreAsync(ColUsers, user.Id, user);

    /// <summary>
    /// Actualiza los datos del perfil del usuario, incluyendo experiencia y nivel.
    /// </summary>
    public async Task UpdateUser(User user)
    {
        var fields = new Dictionary<string, object>
        {
            { "Username",           user.Username },
            { "SelectedCharacterId", user.SelectedCharacterId },
            { "GameHistory",        user.GameHistory },
            { "Experience",         user.Experience },
            { "Level",              user.Level },
            { "XpClaimedParties",   user.XpClaimedParties ?? new List<string>() }
        };

        try
        {
            await _firestore.Collection(ColUsers).Document(user.Id).UpdateAsync(fields);
        }
        catch (Exception ex) when (IsDocumentNotFound(ex))
        {
            // Primera vez: el documento aún no existe, lo creamos completo
            await SaveUser(user);
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Failed to update user {user.Id}", ex);
        }
    }

    public async Task<User?> GetUser(string id)
        => await GetAsync<User>(ColUsers, id);

    /// <summary>
    /// Añade atómicamente un roomCode al historial del usuario
    /// sin leer el documento completo (usa ArrayUnion).
    /// </summary>
    public async Task AddPartyToUserHistoryAsync(string userId, string roomCode)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roomCode)) return;

        try
        {
            await _firestore.Collection(ColUsers).Document(userId).UpdateAsync(
                new Dictionary<string, object>
                {
                    { "GameHistory", FieldValue.ArrayUnion(roomCode) }
                });
        }
        catch (Exception ex)
        {
            Log($"AddPartyToUserHistoryAsync({userId}, {roomCode}): {ex.Message}");
        }
    }

    // ── Juegos ────────────────────────────────────────────────────────────────

    public async Task SaveGame(Game game)
        => await StoreAsync(ColGames, game.Id, game);

    public async Task<List<Game>> GetAvailableGames()
        => await ListAsync<Game>(ColGames);

    // ── Personajes ────────────────────────────────────────────────────────────

    public async Task<List<Character>> GetCharacters()
        => await ListAsync<Character>(ColCharacters);

    // ── Salas (parties) ───────────────────────────────────────────────────────

    public async Task CreateParty(Party party)
        => await StoreAsync(ColParties, party.RoomCode, party);

    public async Task<Party?> GetParty(string roomCode)
        => await GetAsync<Party>(ColParties, roomCode);

    /// <summary>
    /// Comprueba si una sala existe y está activa (no cerrada).
    /// </summary>
    public async Task<bool> CheckLobbyExistsAsync(string roomCode)
    {
        var snapshot = await _firestore
            .Collection(ColParties)
            .WhereEqualsTo("RoomCode", roomCode)
            .LimitTo(1)
            .GetAsync();

        if (snapshot.IsEmpty) return false;

        var party = snapshot.Documents.First().ToObject<Party>();
        return party != null && party.GameState != StateClosed;
    }

    /// <summary>
    /// Actualiza el progreso (puntuaciones) de una sala activa.
    /// </summary>
    public async Task UpdatePartyProgress(string roomCode, PartyProgress progress)
    {
        if (string.IsNullOrEmpty(roomCode)) return;

        var updates = new Dictionary<string, object>
        {
            { "PlayerScores", progress.PlayerScores }
        };

        if (!string.IsNullOrEmpty(progress.WinnerId))
            updates["WinnerId"] = progress.WinnerId;

        try
        {
            await _firestore.Collection(ColParties).Document(roomCode).UpdateAsync(updates);
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Failed to update party progress for {roomCode}", ex);
        }
    }

    /// <summary>
    /// Registra la participación de un usuario en una sala.
    /// Delega en <see cref="AddPartyToUserHistoryAsync"/> para máxima atomicidad.
    /// </summary>
    public async Task JoinLobbyAsync(string userId, string roomCode)
        => await AddPartyToUserHistoryAsync(userId, roomCode);

    /// <summary>
    /// Devuelve las partidas cerradas en las que ha participado el usuario,
    /// ordenadas de más reciente a más antigua. Lee desde el historial del perfil
    /// para evitar problemas si Unity limpió los PlayerIds al cerrar el socket.
    /// </summary>
    public async Task<List<Party>> GetUserHistory(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentException("userId cannot be null or empty.", nameof(userId));

        try
        {
            var user = await GetUser(userId);
            if (user == null || user.GameHistory == null || user.GameHistory.Count == 0)
                return new List<Party>();

            var result = new List<Party>();
            
            // Limitamos a las últimas 20 partidas para no saturar lecturas
            var recentHistory = user.GameHistory.TakeLast(20).Reverse().ToList();

            foreach (var roomCode in recentHistory)
            {
                var party = await GetParty(roomCode);
                if (party != null && party.GameState == StateClosed)
                {
                    result.Add(party);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new DatabaseException($"Failed to retrieve history for user {userId}", ex);
        }
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    /// <summary>
    /// Deserializa un documento de Party garantizando que sus colecciones nunca son null.
    /// </summary>
    private static Party? TryDeserializeParty(IDocumentSnapshot doc)
    {
        try
        {
            var party = doc.ToObject<Party>();
            if (party == null) return null;

            party.PlayerScores          ??= new Dictionary<string, int>();
            party.ParticipantNames      ??= new Dictionary<string, string>();
            party.ParticipantCharacters ??= new Dictionary<string, string>();
            party.PlayedGames           ??= new List<string>();
            party.PlayerIds             ??= new List<string>();
            return party;
        }
        catch (Exception ex)
        {
            Log($"TryDeserializeParty({doc.Id}): {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Heurístico para detectar errores de "documento no existe" sin depender de un tipo concreto.
    /// </summary>
    private static bool IsDocumentNotFound(Exception ex)
        => ex.Message.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("No document", StringComparison.OrdinalIgnoreCase);

    private static void Log(string message)
        => System.Diagnostics.Debug.WriteLine($"[Database] {message}");
}
