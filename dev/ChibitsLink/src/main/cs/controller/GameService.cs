using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChibitsLink.main.controller;

/// <summary>
/// Controla los servicios de conexión entre minijuegos y su funcionamiento
/// </summary>
public class GameController
{
    private readonly GameService _gameService;

    public GameController(GameService gameService)
    {
        _gameService = gameService;
    }

    public async Task<List<Game>> RecoverAvailableGames()
    {
        return await _gameService.GetAvailableGames();
    }

    public async Task<bool> IsLobbyValid(string roomCode)
    {
        return await _gameService.ValidateLobbyAsync(roomCode);
    }
}