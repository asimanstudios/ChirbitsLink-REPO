namespace ChibitsLink.main.cs.service;

using System.Threading.Tasks;
using System.Collections.Generic;
using ChibitsLink.main.cs.model;

public class GameService
{
    private readonly ChibitsLink.main.repository.Database _db;

    public GameService(ChibitsLink.main.repository.Database db)
    {
        _db = db;
    }

    public async Task<List<Game>> GetAvailableGames()
    {
        // Lógica de negocio: Obtener juegos y filtrar o procesar si es necesario
        return await _db.GetAvailableGames();
    }
}