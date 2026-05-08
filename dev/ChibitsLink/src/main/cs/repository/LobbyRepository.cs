using System;
using System.Linq;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository.interfaces;
using ChibitsLink.main.cs.exception;

namespace ChibitsLink.main.repository;

public class LobbyRepository : ILobbyRepository
{
    private readonly IFirestore _firestore;
    private const string CollectionName = "parties";

    public LobbyRepository(FirebaseConnection connection)
    {
        _firestore = connection?.Firestore ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task CreatePartyAsync(Party party)
    {
        try
        {
            await _firestore.Collection(CollectionName).Document(party.RoomCode).SetAsync(party);
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al crear la sala", ex, CollectionName, party.RoomCode);
        }
    }

    public async Task<Party?> GetPartyAsync(string roomCode)
    {
        try
        {
            var snapshot = await _firestore.Collection(CollectionName).Document(roomCode).GetAsync();
            return snapshot.Exists ? snapshot.ToObject<Party>() : null;
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al obtener la sala", ex, CollectionName, roomCode);
        }
    }

    public async Task<bool> ExistsAsync(string roomCode)
    {
        try
        {
            var snapshot = await _firestore.Collection(CollectionName)
                .WhereEqualsTo("RoomCode", roomCode)
                .LimitTo(1)
                .GetAsync();

            if (snapshot.IsEmpty) return false;

            var party = snapshot.Documents.First().ToObject<Party>();
            return party != null && party.GameState != "CLOSED";
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al validar existencia de la sala", ex, CollectionName, roomCode);
        }
    }

    public IDisposable ListenToParty(string roomCode, Action<Party?> onChanged)
    {
        return _firestore.Collection(CollectionName)
            .Document(roomCode)
            .AddSnapshotListener((snapshot, error) =>
            {
                if (error != null || snapshot == null) return;
                onChanged(snapshot.Exists ? snapshot.ToObject<Party>() : null);
            });
    }

    public async Task ToggleReadyAsync(string roomCode, string userId, bool isReady)
    {
        try
        {
            var doc = _firestore.Collection(CollectionName).Document(roomCode);
            if (isReady)
            {
                await doc.UpdateAsync("ReadyPlayerIds", FieldValue.ArrayUnion(userId));
            }
            else
            {
                await doc.UpdateAsync("ReadyPlayerIds", FieldValue.ArrayRemove(userId));
            }
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al actualizar estado de listo", ex, CollectionName, roomCode);
        }
    }
}
