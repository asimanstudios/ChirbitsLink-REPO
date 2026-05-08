using System.Threading.Tasks;
using ChibitsLink.main.cs.model;
using System.Collections.Generic;

namespace ChibitsLink.main.repository.interfaces;

public interface IMasterDataRepository
{
    Task<List<Character>> GetCharactersAsync();
    Task<List<Game>> GetGamesAsync();
    Task InitializeCharactersAsync();
    Task SaveGameAsync(Game game);
}
