using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository.interfaces;

namespace ChibitsLink.main.repository;

/// <summary>
/// Clase responsable de poblar la base de datos con datos iniciales.
/// Refactorizado para usar interfaces de repositorio (Fase 2).
/// </summary>
public class DatabaseSeeder
{
    private readonly IMasterDataRepository _masterRepo;

    public DatabaseSeeder(IMasterDataRepository masterRepo)
    {
        _masterRepo = masterRepo;
    }

    /// <summary>
    /// Ejecuta el sembrado completo.
    /// </summary>
    public async Task SeedAllAsync()
    {
        await _masterRepo.InitializeCharactersAsync();
        await SeedGamesAsync();
    }

    /// <summary>
    /// Puebla la colección de juegos disponibles.
    /// </summary>
    public async Task SeedGamesAsync()
    {
        var games = new List<Game>
        {
            new Game { Id = "Minigame_Runner", Name = "Carrera de Obstáculos", Type = GameType.Jump,          Description = "¡Sé el más rápido!", ImageUrl = "char_placeholder" },
            new Game { Id = "Minigame_Combat", Name = "Arena de Combate",    Type = GameType.Soccer,        Description = "Sobrevive al caos.",  ImageUrl = "char_placeholder" },
            new Game { Id = "Minigame_Soccer", Name = "Fútbol Chibit",     Type = GameType.Soccer,        Description = "Partido de fútbol rápido con físicas locas.", ImageUrl = "char_placeholder" },
            new Game { Id = "Minigame_Kitchen",Name = "Cocina Caótica",    Type = GameType.Kitchen,       Description = "Prepara platos antes de que se acabe el tiempo.", ImageUrl = "char_placeholder" }
        };

        foreach (var g in games)
        {
            await _masterRepo.SaveGameAsync(g);
        }
    }
}
