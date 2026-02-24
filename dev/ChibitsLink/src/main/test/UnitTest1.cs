using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository;

namespace ChibitsLink.main.test;

/// <summary>
/// Pruebas unitarias manuales de los servicios principales.
/// Para ejecutarlas en un entorno de test completo, mueve esta clase a un proyecto xUnit/MSTest independiente.
/// </summary>
public class GameServiceTests
{
    /// <summary>
    /// Verifica que ValidateLobbyAsync devuelve false cuando se pasa una cadena vacía.
    /// </summary>
    public static async Task ValidateLobbyAsync_ShouldReturnFalse_WhenRoomCodeIsEmpty()
    {
        var db = new Database(null!);
        var service = new GameService(db);

        bool result = await service.ValidateLobbyAsync("");

        if (!result == false)
            throw new Exception("FAILED: ValidateLobbyAsync debería devolver false para código vacío.");

        Console.WriteLine("PASSED: ValidateLobbyAsync devuelve false para código vacío.");
    }

    /// <summary>
    /// Verifica que ValidateLobbyAsync devuelve false cuando el código no tiene 6 dígitos.
    /// </summary>
    public static async Task ValidateLobbyAsync_ShouldReturnFalse_WhenCodeTooShort()
    {
        var db = new Database(null!);
        var service = new GameService(db);

        bool result = await service.ValidateLobbyAsync("123");

        if (result)
            throw new Exception("FAILED: ValidateLobbyAsync debería devolver false para código corto.");

        Console.WriteLine("PASSED: ValidateLobbyAsync devuelve false para código corto.");
    }
}