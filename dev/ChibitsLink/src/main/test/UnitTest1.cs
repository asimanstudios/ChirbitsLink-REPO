using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChibitsLink.main.cs.service;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository;
using ChibitsLink.main.repository.interfaces;

namespace ChibitsLink.main.test;

/// <summary>
/// Pruebas unitarias manuales de los servicios principales.
/// </summary>
public class GameServiceTests
{
    public static async Task ValidateLobbyAsync_ShouldReturnFalse_WhenRoomCodeIsEmpty()
    {
        // Usamos null! para los repositorios en este test simple que solo valida formato
        var service = new GameService(null!, null!, null!);

        bool result = await service.ValidateLobbyAsync("");

        if (result)
            throw new Exception("FAILED: ValidateLobbyAsync debería devolver false para código vacío.");

        Console.WriteLine("PASSED: ValidateLobbyAsync devuelve false para código vacío.");
    }

    public static async Task ValidateLobbyAsync_ShouldReturnFalse_WhenCodeTooShort()
    {
        var service = new GameService(null!, null!, null!);

        bool result = await service.ValidateLobbyAsync("123");

        if (result)
            throw new Exception("FAILED: ValidateLobbyAsync debería devolver false para código corto.");

        Console.WriteLine("PASSED: ValidateLobbyAsync devuelve false para código corto.");
    }
}