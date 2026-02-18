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

    // --- MÉTODOS GENÉRICOS ---

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

    // --- LÓGICA DE NEGOCIO ---

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

    // --- NUEVAS FUNCIONES FASE 3 ---

    public async Task<List<Character>> GetCharacters() => await ListAsync<Character>("personajes");
    


    public async Task<List<LobbyHistory>> GetUserHistory(string userId)
    {
        var querySnapshot = await _connection.Firestore
            .Collection("lobbys")
            .WhereEqualsTo("UserId", userId)
            .GetAsync();

        return querySnapshot.Documents.Select(d => d.ToObject<LobbyHistory>()).ToList();
    }
}
