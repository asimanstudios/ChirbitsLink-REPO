using System.Collections.Generic;
using System.Threading.Tasks;
using ChibitsLink.main.cs.exception;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.cs.service;

/// <summary>
/// Gestiona la lógica relacionada con las salas de juego (lobbies) y los juegos disponibles.
/// </summary>
public class GameService
{
    private readonly ChibitsLink.main.repository.Database _db;

    public GameService(ChibitsLink.main.repository.Database db)
    {
        _db = db;
    }

    /// <summary>
    /// Recupera la lista de juegos disponibles desde Firestore.
    /// Lanza <see cref="DatabaseException"/> si hay un error de comunicación.
    /// </summary>
    public async Task<List<Game>> GetAvailableGames()
    {
        try
        {
            return await _db.GetAvailableGames();
        }
        catch (DatabaseException)
        {
            throw;
        }
    }

    /// <summary>
    /// Valida si una sala de juego con el código indicado existe en Firestore.
    /// </summary>
    /// <param name="roomCode">Código de sala de 6 caracteres.</param>
    /// <returns>True si la sala existe; false en caso contrario.</returns>
    public async Task<bool> ValidateLobbyAsync(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode))
            return false;

        try
        {
            return await _db.CheckLobbyExistsAsync(roomCode);
        }
        catch (DatabaseException)
        {
            throw;
        }
    }

    /// <summary>
    /// Recupera la lista de personajes configurados en Firestore.
    /// </summary>
    public async Task<List<Character>> GetCharacters()
    {
        try
        {
            return await _db.GetCharacters();
        }
        catch (DatabaseException)
        {
            throw;
        }
    }

    /// <summary>
    /// Escucha cambios en el lobby en tiempo real.
    /// </summary>
    public IDisposable ListenToLobby(string roomCode, Action<Party?> onChanged)
    {
        return _db.ListenAsync<Party>("parties", roomCode, onChanged);
    }

    /// <summary>
    /// Registra la participación de un usuario en una sala para que aparezca en su historial.
    /// </summary>
    public async Task RegisterParticipationAsync(string userId, string roomCode)
    {
        try
        {
            await _db.JoinLobbyAsync(userId, roomCode);
        }
        catch (DatabaseException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al registrar participación: {ex.Message}");
        }
    }
}