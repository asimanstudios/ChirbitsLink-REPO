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
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException("Error al crear la sala", ex, CollectionName, party.RoomCode);
        }
    }

    public async Task<Party?> GetPartyAsync(string roomCode)
    {
        Party? party = null;
        try
        {
            var snapshot = await _firestore.Collection(CollectionName).Document(roomCode).GetAsync();
            party = snapshot.Exists ? snapshot.ToObject<Party>() : null;
        }
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException("Error al obtener la sala", ex, CollectionName, roomCode);
        }
        return party;
    }

    public async Task<bool> ExistsAsync(string roomCode)
    {
        bool exists = false;
        try
        {
            var snapshot = await _firestore.Collection(CollectionName)
                .WhereEqualsTo("RoomCode", roomCode)
                .LimitTo(1)
                .GetAsync();

            if (!snapshot.IsEmpty)
            {
                var party = snapshot.Documents.First().ToObject<Party>();
                exists = party != null && party.GameState != "CLOSED";
            }
        }
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException("Error al validar existencia de la sala", ex, CollectionName, roomCode);
        }
        return exists;
    }

    public IDisposable ListenToParty(string roomCode, Action<Party?> onChanged)
    {
        return _firestore.Collection(CollectionName)
            .Document(roomCode)
            .AddSnapshotListener((snapshot, error) =>
            {
                if (error == null && snapshot != null)
                {
                    onChanged(snapshot.Exists ? snapshot.ToObject<Party>() : null);
                }
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
        catch (Plugin.CloudFirestore.CloudFirestoreException ex)
        {
            throw new DatabaseException("Error al actualizar estado de listo", ex, CollectionName, roomCode);
        }
    }
}
