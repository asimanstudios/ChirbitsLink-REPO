using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository.interfaces;
using ChibitsLink.main.cs.exception;

namespace ChibitsLink.main.repository;

public class UserRepository : IUserRepository
{
    private readonly IFirestore _firestore;
    private readonly ILobbyRepository _lobbyRepo;
    private const string CollectionName = "users";

    public UserRepository(FirebaseConnection connection, ILobbyRepository lobbyRepo)
    {
        _firestore = connection?.Firestore ?? throw new ArgumentNullException(nameof(connection));
        _lobbyRepo = lobbyRepo;
    }

    public async Task<User?> GetUserAsync(string id)
    {
        User? user = null;
        try
        {
            var snapshot = await _firestore.Collection(CollectionName).Document(id).GetAsync();
            user = snapshot.Exists ? snapshot.ToObject<User>() : null;
        }
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException("Error al obtener usuario", ex, CollectionName, id);
        }
        return user;
    }

    public async Task SaveUserAsync(User user)
    {
        try
        {
            await _firestore.Collection(CollectionName).Document(user.Id).SetAsync(user);
        }
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException("Error al guardar usuario", ex, CollectionName, user.Id);
        }
    }

    public async Task UpdateUserAsync(User user)
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
            await _firestore.Collection(CollectionName).Document(user.Id).UpdateAsync(fields);
        }
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException("Error al actualizar perfil de usuario", ex, CollectionName, user.Id);
        }
    }

    public async Task AddToHistoryAsync(string userId, string roomCode)
    {
        try
        {
            await _firestore.Collection(CollectionName).Document(userId).UpdateAsync(
                new Dictionary<string, object>
                {
                    { "GameHistory", FieldValue.ArrayUnion(roomCode) }
                });
        }
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException("Error al añadir al historial", ex, CollectionName, userId);
        }
    }

    public async Task<List<Party>> GetUserHistoryAsync(string userId)
    {
        try
        {
            var result = new List<Party>();
            var user = await GetUserAsync(userId);
            if (user != null && user.GameHistory != null && user.GameHistory.Count > 0)
            {
                var recentHistory = user.GameHistory.TakeLast(20).Reverse().ToList();
                Party? party;
                foreach (var roomCode in recentHistory)
                {
                    party = await _lobbyRepo.GetPartyAsync(roomCode);
                    if (party != null && party.GameState == "CLOSED")
                    {
                        result.Add(party);
                    }
                }
            }
            return result;
        }
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException($"Error al recuperar historial del usuario {userId}", ex, CollectionName, userId);
        }
    }
}
