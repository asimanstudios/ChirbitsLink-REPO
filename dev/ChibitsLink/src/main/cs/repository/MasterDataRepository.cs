using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.CloudFirestore;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository.interfaces;
using ChibitsLink.main.cs.exception;

namespace ChibitsLink.main.repository;

public class MasterDataRepository : IMasterDataRepository
{
    private readonly IFirestore _firestore;
    private const string ColCharacters = "characters";
    private const string ColGames = "games";

    private List<Character>? _cachedCharacters = null;
    private List<Game>? _cachedGames = null;

    public MasterDataRepository(FirebaseConnection connection)
    {
        _firestore = connection?.Firestore ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<List<Character>> GetCharactersAsync()
    {
        if (_cachedCharacters != null) return _cachedCharacters;

        try
        {
            var snapshot = await _firestore.Collection(ColCharacters).GetAsync();
            _cachedCharacters = snapshot.Documents.Select(d => d.ToObject<Character>()).Where(x => x != null).Cast<Character>().ToList();
            return _cachedCharacters;
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al listar personajes", ex, ColCharacters);
        }
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        if (_cachedGames != null) return _cachedGames;

        try
        {
            var snapshot = await _firestore.Collection(ColGames).GetAsync();
            _cachedGames = snapshot.Documents.Select(d => d.ToObject<Game>()).Where(x => x != null).Cast<Game>().ToList();
            return _cachedGames;
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al listar minijuegos", ex, ColGames);
        }
    }

    public async Task InitializeCharactersAsync()
    {
        try
        {
            var existing = await GetCharactersAsync();
            if (existing.Count > 0) return;

            var defaults = new List<Character>
            {
                new Character { Id = "VALIENTE",    Name = "Valiente",    Description = "Guerrero audaz.", ImageUrl = "char_placeholder" },
                new Character { Id = "MAGO",        Name = "Mago",        Description = "Maestro arcano.", ImageUrl = "char_placeholder" },
                new Character { Id = "EXPLORADOR",  Name = "Explorador",  Description = "Sigiloso.",       ImageUrl = "char_placeholder" },
                new Character { Id = "CURADORA",    Name = "Curadora",    Description = "Apoyo.",          ImageUrl = "char_placeholder" },
                new Character { Id = "TANQUE",      Name = "Tanque",      Description = "Defensa.",        ImageUrl = "char_placeholder" }
            };

            foreach (var c in defaults)
            {
                await _firestore.Collection(ColCharacters).Document(c.Id).SetAsync(c);
            }
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al inicializar catálogo", ex, ColCharacters);
        }
    }

    public async Task SaveGameAsync(Game game)
    {
        try
        {
            await _firestore.Collection(ColGames).Document(game.Id).SetAsync(game);
        }
        catch (Exception ex)
        {
            throw new DatabaseException("Error al guardar minijuego", ex, ColGames, game.Id);
        }
    }
}
