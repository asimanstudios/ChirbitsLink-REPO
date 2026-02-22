using Plugin.CloudFirestore;
using ChibitsLink.main.cs.model;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StoreAsync Error ({collection}): {ex.Message}");
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

            return snapshot.Exists ? snapshot.ToObject<T>() : null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAsync Error ({collection}/{documentId}): {ex.Message}");
            return null;
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
            return new List<T>();
        }
    }

    public async Task DeleteAsync(string collection, string documentId)
    {
        await _connection.Firestore
            .Collection(collection)
            .Document(documentId)
            .DeleteAsync();
    }

    public async Task SaveUser(User user) => await StoreAsync("users", user.Id, user);
    public async Task UpdateUser(User user) => await SaveUser(user);
    public async Task<User?> GetUser(string id) => await GetAsync<User>("users", id);
    
    public async Task SaveGame(Game game) => await StoreAsync("games", game.Id.ToString(), game);
    public async Task<List<Game>> GetAvailableGames() => await ListAsync<Game>("games");

    public async Task CreateParty(Party party) => await StoreAsync("parties", party.RoomCode, party);
    public async Task<Party?> GetParty(string code) => await GetAsync<Party>("parties", code);

    /// <summary>
    /// Actualiza los datos de una sala existente.
    /// </summary>
    public async Task UpdateParty(Party party)
    {
        if (party == null || string.IsNullOrEmpty(party.RoomCode))
            return;
            
        try
        {
            var updates = new Dictionary<string, object>();
            
            if (party.Name != null)
                updates["Name"] = party.Name;
                
            updates["CurrentPlayers"] = party.CurrentPlayers;
            updates["PlayerIds"] = party.PlayerIds ?? new List<string>();
            
            if (updates.Count > 0)
            {
                await _connection.Firestore
                    .Collection("parties")
                    .Document(party.RoomCode)
                    .UpdateAsync(updates);
                    
                System.Diagnostics.Debug.WriteLine($"[Database] Sala {party.RoomCode} actualizada: {party.CurrentPlayers} jugadores");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Database] Error actualizando sala: {ex.Message}");
        }
    }

    public async Task UpdatePartyProgress(string roomCode, PartyProgress progress)
    {
        await _connection.Firestore
            .Collection("parties")
            .Document(roomCode)
            .UpdateAsync(new { progress = progress });
    }

    public async Task<List<Character>> GetCharacters() => await ListAsync<Character>("personajes");

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
    /// Inicializa los personajes en la base de datos si no existen.
    /// Esto permite al usuario meter las imágenes después.
    /// </summary>
    public async Task InitializeCharactersAsync()
    {
        var existing = await GetCharacters();
        if (existing.Count > 0) return;

        var characters = new List<Character>
        {
            new Character { Id = "VALIENTE", Name = "Valiente", Description = "Guerrero audaz", Attack = 15, Defense = 10, Speed = 8 },
            new Character { Id = "MAGO", Name = "Mago", Description = "Maestro de las artes oscuras", Attack = 20, Defense = 5, Speed = 10 },
            new Character { Id = "EXPLORADOR", Name = "Explorador", Description = "Rápido y letal", Attack = 12, Defense = 7, Speed = 18 }
        };

        foreach (var c in characters)
        {
            await StoreAsync("personajes", c.Id, c);
        }
    }

    /// <summary>
    /// Registra la participación de un usuario en un lobby.
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

        await StoreAsync("lobbys", history.Id, history);

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
            .Collection("lobbys")
            .WhereEqualsTo("UserId", userId)
            .OrderBy("Timestamp", true)
            .GetAsync();

        return querySnapshot.Documents.Select(d => d.ToObject<LobbyHistory>()).ToList();
    }
}
