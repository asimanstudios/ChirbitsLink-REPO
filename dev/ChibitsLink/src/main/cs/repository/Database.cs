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
    private readonly Connection _connection;

    public Database(Connection connection)
    {
        _connection = connection;
    }

    // --- MÉTODOS GENÉRICOS ---

    public async Task StoreAsync<T>(string collection, string documentId, T data) where T : class
    {
        await _connection.Firestore
            .Collection(collection)
            .Document(documentId)
            .SetAsync(data);
    }

    public async Task<T?> GetAsync<T>(string collection, string documentId) where T : class
    {
        var snapshot = await _connection.Firestore
            .Collection(collection)
            .Document(documentId)
            .GetAsync();

        return snapshot.Exists ? snapshot.ToObject<T>() : null;
    }

    public async Task<List<T>> ListAsync<T>(string collection) where T : class
    {
        var querySnapshot = await _connection.Firestore
            .Collection(collection)
            .GetAsync();

        return querySnapshot.Documents.Select(d => d.ToObject<T>()).ToList();
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
}
