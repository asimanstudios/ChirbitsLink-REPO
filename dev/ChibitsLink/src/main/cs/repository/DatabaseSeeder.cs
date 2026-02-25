using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.repository;

/// <summary>
/// Clase encargada de poblar la base de datos con datos iniciales.
/// </summary>
public class DatabaseSeeder
{
    private readonly Database _database;

    public DatabaseSeeder(Database database)
    {
        _database = database;
    }

    /// <summary>
    /// Ejecuta el sembrado completo de la base de datos.
    /// </summary>
    public async Task SeedAllAsync()
    {
        await SeedCharactersAsync();
        await SeedGamesAsync();
    }

    /// <summary>
    /// Puebla la colección de personajes.
    /// </summary>
    public async Task SeedCharactersAsync()
    {
        var characters = new List<Character>
        {
            new Character { Id = "VALIENTE",    Name = "Valiente",    Description = "Guerrero audaz, fuerte en combate cuerpo a cuerpo.",   ImageUrl = "char_valiente.png" },
            new Character { Id = "MAGO",        Name = "Mago",        Description = "Maestro de las artes arcanas, poder mágico superior.", ImageUrl = "char_mago.png"     },
            new Character { Id = "EXPLORADOR",  Name = "Explorador",  Description = "Rápido y sigiloso, experto en movimiento y evasión.",  ImageUrl = "char_explorador.png" },
            new Character { Id = "CURADORA",    Name = "Curadora",    Description = "Especialista en sanación y apoyo al equipo.",          ImageUrl = "char_curadora.png"   },
            new Character { Id = "TANQUE",      Name = "Tanque",      Description = "Defensa impenetrable, capaz de absorber mucho daño.",    ImageUrl = "char_tanque.png"     }
        };

        foreach (var c in characters)
        {
            await _database.StoreAsync("personajes", c.Id, c);
        }
    }

    /// <summary>
    /// Puebla la colección de juegos disponibles.
    /// </summary>
    public async Task SeedGamesAsync()
    {
        var games = new List<Game>
        {
            new Game { Id = 1, Name = "Fútbol Chibit",     Type = GameType.Soccer,        Description = "Partido de fútbol rápido con físicas locas." },
            new Game { Id = 2, Name = "Salto Infinito",    Type = GameType.Jump,          Description = "Llega lo más alto posible esquivando obstáculos." },
            new Game { Id = 3, Name = "Equilibrio G",      Type = GameType.Accelerometer, Description = "Controla la plataforma con el acelerómetro." },
            new Game { Id = 4, Name = "Cocina Caótica",    Type = GameType.Kitchen,       Description = "Prepara platos antes de que se acabe el tiempo." }
        };

        foreach (var g in games)
        {
            await _database.SaveGame(g);
        }
    }
}
