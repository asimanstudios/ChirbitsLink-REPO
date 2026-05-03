using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChibitsLink.main.cs.exception;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository.interfaces;

namespace ChibitsLink.main.cs.service;

/// <summary>
/// Gestiona la lógica relacionada con las salas de juego (lobbies) y los juegos disponibles.
/// Refactorizado para usar repositorios modulares (Fase 2).
/// </summary>
public class GameService
{
    private readonly ILobbyRepository _lobbyRepo;
    private readonly IMasterDataRepository _masterRepo;
    private readonly IUserRepository _userRepo;

    public GameService(ILobbyRepository lobbyRepo, IMasterDataRepository masterRepo, IUserRepository userRepo)
    {
        _lobbyRepo = lobbyRepo;
        _masterRepo = masterRepo;
        _userRepo = userRepo;
    }

    /// <summary>
    /// Recupera la lista de juegos disponibles desde Firestore.
    /// </summary>
    public async Task<List<Game>> GetAvailableGames()
    {
        return await _masterRepo.GetGamesAsync();
    }

    /// <summary>
    /// Valida si una sala de juego con el código indicado existe en Firestore.
    /// </summary>
    public async Task<bool> ValidateLobbyAsync(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode)) return false;
        return await _lobbyRepo.ExistsAsync(roomCode);
    }

    /// <summary>
    /// Recupera la lista de personajes configurados en Firestore.
    /// </summary>
    public async Task<List<Character>> GetCharacters()
    {
        return await _masterRepo.GetCharactersAsync();
    }

    /// <summary>
    /// Escucha cambios en el lobby en tiempo real.
    /// </summary>
    public IDisposable ListenToLobby(string roomCode, Action<Party?> onChanged)
    {
        return _lobbyRepo.ListenToParty(roomCode, onChanged);
    }

    /// <summary>
    /// Registra la participación de un usuario en una sala para que aparezca en su historial.
    /// </summary>
    public async Task RegisterParticipationAsync(string userId, string roomCode)
    {
        try
        {
            await _userRepo.AddToHistoryAsync(userId, roomCode);
        }
        catch (DatabaseException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GameService] Error al registrar participación: {ex.Message}");
        }
    }

    /// <summary>
    /// Cambia el estado de preparación de un jugador en la sala.
    /// </summary>
    public async Task ToggleReadyAsync(string roomCode, string userId, bool isReady)
    {
        await _lobbyRepo.ToggleReadyAsync(roomCode, userId, isReady);
    }
}