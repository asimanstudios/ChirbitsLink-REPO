using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using ChibitsLink.main.cs.exception;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.repository;

/// <summary>
/// Gestiona las operaciones CRUD sobre Cloud Firestore.
/// </summary>
public class Database
{
    private readonly FirebaseConnection _connection;

    public Database(FirebaseConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    /// Escucha cambios en tiempo real de un documento de Firestore.
    /// </summary>
    public IDisposable ListenAsync<T>(string collection, string documentId, Action<T?> onChanged) where T : class
    {
        return _connection.Firestore
            .Collection(collection)
            .Document(documentId)
            .AddSnapshotListener((snapshot, error) =>
            {
                if (error != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ListenAsync Error ({collection}/{documentId}): {error.Message}");
                    return;
                }

                if (snapshot != null && snapshot.Exists)
                {
                    onChanged(snapshot.ToObject<T>());
                }
                else
                {
                    onChanged(null);
                }
            });
    }

    // --- MÉTODOS CRUD ---

    public async Task StoreAsync<T>(string collection, string documentId, T data) where T : class
    {
        try
        {
            await _connection.Firestore
                .Collection(collection)
                .Document(documentId)
                .SetAsync(data);
        }
        catch (ArgumentException ex)
        {
            throw new DatabaseException($"Invalid argument while storing to {collection}", ex);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StoreAsync Error ({collection}): {ex.Message}");
            throw new DatabaseException($"Failed to store document in {collection}", ex);
        }
    }

    public async Task<T?> GetAsync<T>(string collection, string documentId) where T : class
    {
        try
        {
            var snapshot = await _connection.Firestore
                .Collection(collection)
                .Document(documentId)
                .GetAsync();

            if (!snapshot.Exists)
            {
                return null;
            }

            return snapshot.ToObject<T>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAsync Error ({collection}/{documentId}): {ex.Message}");
            throw new DatabaseException($"Failed to retrieve document {documentId} from {collection}", ex);
        }
    }

    public async Task<List<T>> ListAsync<T>(string collection) where T : class
    {
        try
        {
            var querySnapshot = await _connection.Firestore
                .Collection(collection)
                .GetAsync();

            return querySnapshot.Documents.Select(d => d.ToObject<T>()).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ListAsync Error ({collection}): {ex.Message}");
            throw new DatabaseException($"Failed to list documents in {collection}", ex);
        }
    }

    public async Task DeleteAsync(string collection, string documentId)
    {
        try
        {
            await _connection.Firestore
                .Collection(collection)
                .Document(documentId)
                .DeleteAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteAsync Error ({collection}/{documentId}): {ex.Message}");
            throw new DatabaseException($"Failed to delete document {documentId} from {collection}", ex);
        }
    }

    public async Task SaveUser(User user) => await StoreAsync("users", user.Id, user);
    public async Task UpdateUser(User user) => await SaveUser(user);
    public async Task<User?> GetUser(string id) => await GetAsync<User>("users", id);
    
    public async Task SaveGame(Game game) => await StoreAsync("games", game.Id.ToString(), game);
    public async Task<List<Game>> GetAvailableGames() => await ListAsync<Game>("games");

    public async Task CreateParty(Party party) => await StoreAsync("parties", party.RoomCode, party);
    public async Task<Party?> GetParty(string code) => await GetAsync<Party>("parties", code);

    public async Task UpdatePartyProgress(string roomCode, PartyProgress progress)
    {
        await _connection.Firestore
            .Collection("parties")
            .Document(roomCode)
            .UpdateAsync(new { progress = progress });
    }

    public async Task<List<Character>> GetCharacters() => await ListAsync<Character>("characters");

    /// <summary>
    /// Verifica si un lobby existe por su código de sala.
    /// </summary>
    public async Task<bool> CheckLobbyExistsAsync(string roomCode)
    {
        var snapshot = await _connection.Firestore
            .Collection("parties")
            .WhereEqualsTo("RoomCode", roomCode)
            .LimitTo(1)
            .GetAsync();
        
        return !snapshot.IsEmpty;
    }

    /// <summary>
    /// Initializes characters in the database if they don't exist.
    /// This allows the user to add images later.
    /// </summary>
    public async Task InitializeCharactersAsync()
    {
        var existing = await GetCharacters();
        if (existing.Count > 0) return;

        var characters = new List<Character>
        {
            new Character { Id = "VALIENTE",    Name = "Valiente",    Description = "Guerrero audaz, fuerte en combate cuerpo a cuerpo.",   ImageUrl = "char_valiente" },
            new Character { Id = "MAGO",        Name = "Mago",        Description = "Maestro de las artes arcanas, poder mágico superior.", ImageUrl = "char_mago"     },
            new Character { Id = "EXPLORADOR",  Name = "Explorador",  Description = "Rápido y sigiloso, experto en movimiento y evasión.",  ImageUrl = "char_explorador" }
        };

        foreach (var c in characters)
        {
            await StoreAsync("characters", c.Id, c);
        }
    }

    /// <summary>
    /// Registers a user's participation in a lobby.
    /// </summary>
    public async Task JoinLobbyAsync(string userId, string roomCode)
    {
        var history = new LobbyHistory
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            RoomCode = roomCode,
            Timestamp = DateTime.UtcNow
        };

        await StoreAsync("lobbies", history.Id, history);

        // También actualizamos el historial directo del usuario por conveniencia
        var user = await GetUser(userId);
        if (user != null)
        {
            if (user.GameHistory == null) user.GameHistory = new List<string>();
            user.GameHistory.Add(roomCode);
            await UpdateUser(user);
        }
    }


    public async Task<List<LobbyHistory>> GetUserHistory(string userId)
    {
        var querySnapshot = await _connection.Firestore
            .Collection("lobbies")
            .WhereEqualsTo("UserId", userId)
            .OrderBy("Timestamp", true)
            .GetAsync();

        return querySnapshot.Documents.Select(d => d.ToObject<LobbyHistory>()).ToList();
    }
}
