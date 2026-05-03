using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using ChibitsLink.main.cs.model;
using ChibitsLink.main.repository.interfaces;
using ChibitsLink.main.repository;
using ChibitsLink.main.cs.exception;

namespace ChibitsLink.main.cs.service;

/// <summary>
/// Gestiona la autenticación de usuarios (inicio de sesión, registro, cierre de sesión)
/// y la persistencia de la sesión activa del usuario.
/// </summary>
public class AccountService : BaseService
{
    private readonly IUserRepository _userRepo;
    private readonly IMasterDataRepository _masterRepo;
    private readonly FirebaseConnection _connection;
    private User? _currentUser;

    private const string SESSION_UID_KEY = "session_uid";
    private const string SESSION_EXPIRY_KEY = "session_expiry";

    public AccountService(IUserRepository userRepo, IMasterDataRepository masterRepo, FirebaseConnection connection)
    {
        _userRepo = userRepo;
        _masterRepo = masterRepo;
        _connection = connection;
    }

    /// <summary>
    /// Comprueba si existe una sesión activa y válida para el usuario.
    /// </summary>
    public async Task<bool> IsSessionActiveAsync()
    {
        try
        {
            var userId = Preferences.Get(SESSION_UID_KEY, string.Empty);
            var expiryString = Preferences.Get(SESSION_EXPIRY_KEY, string.Empty);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(expiryString))
                return false;

            if (DateTime.TryParse(expiryString, out var expiry) && DateTime.Now < expiry)
            {
                _currentUser = await _userRepo.GetUserAsync(userId);
                return _currentUser != null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Session Check Error: {ex.Message}");
        }
        return false;
    }

    private async Task SaveSession(string uid)
    {
        Preferences.Set(SESSION_UID_KEY, uid);
        Preferences.Set(SESSION_EXPIRY_KEY, DateTime.Now.AddDays(30).ToString());
        await Task.CompletedTask;
    }

    /// <summary>
    /// Inicia sesión con email y contraseña mediante Firebase Auth.
    /// Lanza excepciones específicas si ocurre un error.
    /// </summary>
    public async Task Login(string email, string password)
    {
        try
        {
            var result = await _connection.Auth.SignInWithEmailAndPasswordAsync(email, password);
            if (result.User == null)
                throw new AuthException("La autenticación no devolvió un usuario válido.");

            // FIX: Mayor pausa para asegurar propagación
            await Task.Delay(1500);

            _currentUser = await _userRepo.GetUserAsync(result.User.Uid);
            
            if (_currentUser == null)
            {
                _connection.Auth.SignOut();
                throw new DatabaseException($"El perfil Firestore no existe para el UID actual ({result.User.Uid}). Elimina tu cuenta desde la consola de Firebase Auth y regístrate de nuevo.", "users", result.User.Uid);
            }

            _currentUser.Email = result.User.Email ?? email;
            await SaveSession(result.User.Uid);
        }
        catch (DatabaseException)
        {
            throw; // Re-lanzar excepciones de negocio conocidas
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}");
            throw new AuthException($"Error interno durante el login: {ex.Message}");
        }
    }

    /// <summary>
    /// Registra un nuevo usuario en Firebase Auth y crea su perfil en Firestore.
    /// Lanza excepciones si el registro falla.
    /// </summary>
    public async Task RegisterAsync(string realName, string username, string email, string password)
    {
        try
        {
            var result = await _connection.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            if (result.User == null) 
                throw new AuthException("El registro no devolvió un usuario.");

            // FIX: Mayor pausa para asegurar la propagación del Token de Auth a Firestore (Race Condition pesado)
            await Task.Delay(1500);

            var newUser = new User
            {
                Id = result.User.Uid,
                Email = email,
                RealName = realName,
                Username = username
            };

            try 
            {
                await _userRepo.SaveUserAsync(newUser);
            }
            catch (Exception ex)
            {
                // ROLLBACK: Borrar la cuenta vacía de Auth si falla la escritura en BBDD para no dejarla "huérfana"
                await result.User.DeleteAsync();
                throw new DatabaseException($"Error bloqueando en BBDD (probablemente Reglas/Tiempo). Cuenta de Auth cancelada para evitar bucles. Detalle: {ex.Message}");
            }
            _currentUser = newUser;
            await SaveSession(result.User.Uid);

            await _masterRepo.InitializeCharactersAsync();
        }
        catch (DatabaseException)
        {
            throw; // Re-lanzar excepciones conocidas
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Registration Error: {ex.Message}");
            throw new AuthException($"Error al crear la cuenta. Inténtalo de nuevo más tarde: {ex.Message}");
        }
    }

    /// <summary>Actualiza los datos del usuario tanto en Firestore como en memoria.</summary>
    public async Task UpdateUser(User user)
    {
        await _userRepo.UpdateUserAsync(user);
        _currentUser = user;
    }

    /// <summary>Actualiza el email del usuario autenticado en Firebase Auth.</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateEmail(string newEmail)
    {
        try
        {
            if (_connection.Auth.CurrentUser == null) return (false, "No hay sesión activa.");
            await _connection.Auth.CurrentUser.UpdateEmailAsync(newEmail);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>Cambia la contraseña del usuario autenticado en Firebase Auth.</summary>
    public async Task<(bool Success, string? ErrorMessage)> ChangePassword(string newPassword)
    {
        try
        {
            if (_connection.Auth.CurrentUser == null) return (false, "No hay sesión activa.");
            await _connection.Auth.CurrentUser.UpdatePasswordAsync(newPassword);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>Cierra la sesión del usuario y borra los datos de sesión persistidos.</summary>
    public async Task Logout()
    {
        _connection.Auth.SignOut();
        _currentUser = null;
        Preferences.Remove(SESSION_UID_KEY);
        Preferences.Remove(SESSION_EXPIRY_KEY);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Calcula y añade la experiencia ganada en una partida cerrada con la regla de 5k.
    /// Evita doble reclamación usando XpClaimedParties.
    /// </summary>
    public async Task ClaimPartyExperienceAsync(string roomCode, Party party)
    {
        if (_currentUser == null || party == null) return;
        if (_currentUser.XpClaimedParties == null) _currentUser.XpClaimedParties = new List<string>();

        if (_currentUser.XpClaimedParties.Contains(roomCode)) return; // Ya reclamada

        if (party.PlayerScores != null && party.PlayerScores.TryGetValue(_currentUser.Id, out int scoreToAdd))
        {
            if (scoreToAdd <= 0) return;

            int newXp = _currentUser.Experience + scoreToAdd;
            
            // Lógica de Niveles: Nivel N requiere N * 5000 XP para subir
            int calculatedLevel = 1;
            int tempXp = newXp;
            int xpRequired = calculatedLevel * 5000;

            while (tempXp >= xpRequired)
            {
                tempXp -= xpRequired;
                calculatedLevel++;
                xpRequired = calculatedLevel * 5000;
            }

            _currentUser.Experience = newXp;
            _currentUser.Level = calculatedLevel;
            _currentUser.XpClaimedParties.Add(roomCode);

            // Asegurarnos de que también está en el GameHistory por si Unity falló en añadirlo
            if (_currentUser.GameHistory == null) _currentUser.GameHistory = new List<string>();
            if (!_currentUser.GameHistory.Contains(roomCode))
            {
                _currentUser.GameHistory.Add(roomCode);
            }

            await UpdateUser(_currentUser);
            System.Diagnostics.Debug.WriteLine($"[AccountService] XP reclamada para sala {roomCode}. +{scoreToAdd} XP -> Nivel {calculatedLevel}");
        }
    }

    /// <summary>
    /// Revisa el historial del perfil del usuario al abrir el menú y reclama la XP 
    /// de cualquier sala CLOSED de la que aún no hayamos cobrado. (Evita el bug de cierre rápido).
    /// </summary>
    public async Task CheckAndClaimPendingExperienceAsync(ILobbyRepository lobbyRepo)
    {
        if (_currentUser == null) return;
        
        // Refrescar el usuario para tener la lista de GameHistory actualizada desde Firestore
        var freshUser = await _userRepo.GetUserAsync(_currentUser.Id);
        if (freshUser != null)
        {
            _currentUser = freshUser;
        }

        if (_currentUser.GameHistory == null || _currentUser.GameHistory.Count == 0) return;
        if (_currentUser.XpClaimedParties == null) _currentUser.XpClaimedParties = new List<string>();

        // Buscar salas del historial que NO están en XpClaimedParties
        var pendingRooms = _currentUser.GameHistory.Except(_currentUser.XpClaimedParties).ToList();
        
        // Buscar todas las salas de Firestore en paralelo (súper rápido)
        var partyTasks = pendingRooms.Select(code => lobbyRepo.GetPartyAsync(code));
        var parties = await Task.WhenAll(partyTasks);
        
        bool updated = false;
        for (int i = 0; i < pendingRooms.Count; i++)
        {
            var roomCode = pendingRooms[i];
            var party = parties[i];
            
            // Solo reclamamos si la sala ya está cerrada por Unity
            if (party != null && party.GameState == "CLOSED")
            {
                await ClaimPartyExperienceAsync(roomCode, party);
                updated = true;
            }
        }

        // Si actualizamos la XP, volvemos a refrescar para que MainMenu lo lea perfecto
        if (updated)
        {
            var refreshedUser = await _userRepo.GetUserAsync(_currentUser.Id);
            if (refreshedUser != null) _currentUser = refreshedUser;
        }
    }

    /// <summary>Devuelve el usuario actualmente autenticado en memoria, o null si no hay sesión.</summary>
    public User? GetCurrentUser() => _currentUser;
}