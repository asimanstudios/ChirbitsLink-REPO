using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.repository;

/// <summary>
/// Class responsible for seeding the database with initial data.
/// </summary>
public class DatabaseSeeder
{
    private readonly Database _database;

    public DatabaseSeeder(Database database)
    {
        _database = database;
    }

    /// <summary>
    /// Executes the full database seeding.
    /// </summary>
    public async Task SeedAllAsync()
    {
        await SeedCharactersAsync();
        await SeedGamesAsync();
    }

    /// <summary>
    /// Seeds the characters collection.
    /// </summary>
    public async Task SeedCharactersAsync()
    {
        var characters = new List<Character>
        {
            new Character { Id = "VALIENTE",    Name = "Valiente",    Description = "Guerrero audaz, fuerte en combate cuerpo a cuerpo.",   ImageUrl = "char_placeholder" },
            new Character { Id = "MAGO",        Name = "Mago",        Description = "Maestro de las artes arcanas, poder mágico superior.", ImageUrl = "char_placeholder" },
            new Character { Id = "EXPLORADOR",  Name = "Explorador",  Description = "Rápido y sigiloso, experto en movimiento y evasión.",  ImageUrl = "char_placeholder" },
            new Character { Id = "CURADORA",    Name = "Curadora",    Description = "Especialista en sanación y apoyo al equipo.",          ImageUrl = "char_placeholder" },
            new Character { Id = "TANQUE",      Name = "Tanque",      Description = "Defensa impenetrable, capaz de absorber mucho daño.",    ImageUrl = "char_placeholder" }
        };

        foreach (var c in characters)
        {
            await _database.StoreAsync("characters", c.Id, c);
        }
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
            await _database.SaveGame(g);
        }
    }
}
