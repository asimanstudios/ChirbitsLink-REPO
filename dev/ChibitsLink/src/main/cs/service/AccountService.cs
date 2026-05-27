using System;
using System.Collections.Generic;
using System.Linq;
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
        bool result = false;
        var userId = Preferences.Get(SESSION_UID_KEY, string.Empty);
        var expiryString = Preferences.Get(SESSION_EXPIRY_KEY, string.Empty);

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(expiryString))
        {
            bool sessionValid = DateTime.TryParse(expiryString, out var expiry) && DateTime.Now < expiry;
            if (sessionValid)
            {
                try
                {
                    _currentUser = await _userRepo.GetUserAsync(userId);
                    result = _currentUser != null;
                }
                catch (DatabaseException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AccountService] IsSessionActive: Firestore error: {ex.Message}");
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("[AccountService] IsSessionActive: operación cancelada.");
                }
            }
        }

        return result;
    }

    private void SaveSession(string uid)
    {
        Preferences.Set(SESSION_UID_KEY, uid);
        Preferences.Set(SESSION_EXPIRY_KEY, DateTime.Now.AddDays(30).ToString());
    }

    /// <summary>
    /// Inicia sesión con email y contraseña mediante Firebase Auth.
    /// Lanza <see cref="AuthException"/> si las credenciales son incorrectas.
    /// Lanza <see cref="DatabaseException"/> si el perfil de Firestore no existe.
    /// </summary>
    public async Task Login(string email, string password)
    {
        // ── 1. Autenticar contra Firebase Auth ──────────────────────────────
        Plugin.FirebaseAuth.IAuthResult result;
        try
        {
            result = await _connection.Auth.SignInWithEmailAndPasswordAsync(email, password);
        }
        catch (Plugin.FirebaseAuth.FirebaseAuthException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AccountService] Login: credenciales rechazadas: {ex.Message}");
            throw new AuthException("Credenciales incorrectas. Verifica tu email y contraseña.");
        }

        if (result.User == null)
            throw new AuthException("La autenticación no devolvió un usuario válido.");

        // ── 2. Esperar propagación del token antes de leer Firestore ─────────
        await Task.Delay(1500);

        // ── 3. Leer perfil de Firestore (DatabaseException propaga si falla) ─
        _currentUser = await _userRepo.GetUserAsync(result.User.Uid);

        if (_currentUser == null)
        {
            _connection.Auth.SignOut();
            throw new DatabaseException(
                $"El perfil Firestore no existe para el UID ({result.User.Uid}). " +
                "Elimina la cuenta desde la consola de Firebase Auth y regístrate de nuevo.",
                "users", result.User.Uid);
        }

        _currentUser.Email = result.User.Email ?? email;
        SaveSession(result.User.Uid);
    }

    /// <summary>
    /// Registra un nuevo usuario en Firebase Auth y crea su perfil en Firestore.
    /// Lanza <see cref="AuthException"/> si Firebase Auth rechaza el registro.
    /// Lanza <see cref="DatabaseException"/> si falla la escritura en Firestore (con rollback de Auth).
    /// </summary>
    public async Task RegisterAsync(string realName, string username, string email, string password)
    {
        // ── 1. Crear cuenta en Firebase Auth ─────────────────────────────────
        Plugin.FirebaseAuth.IAuthResult result;
        try
        {
            result = await _connection.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        }
        catch (Plugin.FirebaseAuth.FirebaseAuthException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AccountService] Register: error de Auth: {ex.Message}");
            throw new AuthException($"No se pudo crear la cuenta: {ex.Message}");
        }

        if (result.User == null)
            throw new AuthException("El registro no devolvió un usuario.");

        // ── 2. Esperar propagación del token de Auth a Firestore ─────────────
        await Task.Delay(1500);

        var newUser = new User
        {
            Id = result.User.Uid,
            Email = email,
            RealName = realName,
            Username = username
        };

        // ── 3. Guardar perfil en Firestore; rollback si falla ────────────────
        try
        {
            await _userRepo.SaveUserAsync(newUser);
        }
        catch (DatabaseException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AccountService] Register: rollback Auth por fallo en BBDD: {ex.Message}");
            await result.User.DeleteAsync();
            throw new DatabaseException(
                $"Error al guardar el perfil en Firestore (probablemente reglas de seguridad o tiempo de propagación). " +
                $"Cuenta de Auth cancelada para evitar huérfanos. Detalle: {ex.Message}",
                "users", result.User.Uid);
        }

        _currentUser = newUser;
        SaveSession(result.User.Uid);

        // ── 4. Inicializar catálogo de personajes si está vacío ───────────────
        await _masterRepo.InitializeCharactersAsync();
    }

    /// <summary>Actualiza los datos del usuario tanto en Firestore como en memoria.</summary>
    public async Task UpdateUser(User user)
    {
        await _userRepo.UpdateUserAsync(user);
        _currentUser = user;
    }

    /// <summary>
    /// Re-autentica al usuario con sus credenciales actuales.
    /// Firebase exige esto antes de operaciones sensibles (cambiar email o contraseña).
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> ReauthenticateAsync(string currentPassword)
    {
        bool success = false;
        string? errorMessage = null;
        var firebaseUser = _connection.Auth.CurrentUser;

        if (firebaseUser == null)
        {
            errorMessage = "No hay sesión activa.";
        }
        else
        {
            string email = _currentUser?.Email ?? firebaseUser.Email ?? string.Empty;
            if (string.IsNullOrEmpty(email))
            {
                errorMessage = "No se pudo obtener el email de la sesión actual.";
            }
            else
            {
                try
                {
                    var credential = Plugin.FirebaseAuth.CrossFirebaseAuth.Current.EmailAuthProvider.GetCredential(email, currentPassword);
                    await firebaseUser.ReauthenticateAsync(credential);
                    success = true;
                }
                catch (Plugin.FirebaseAuth.FirebaseAuthException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AccountService] ReauthenticateAsync: contraseña incorrecta: {ex.Message}");
                    errorMessage = "Contraseña actual incorrecta.";
                }
            }
        }

        return (success, errorMessage);
    }

    /// <summary>Actualiza el email del usuario autenticado en Firebase Auth.</summary>
    public async Task<(bool Success, string? ErrorMessage)> UpdateEmail(string newEmail)
    {
        bool success = false;
        string? errorMessage = null;

        if (_connection.Auth.CurrentUser == null)
        {
            errorMessage = "No hay sesión activa.";
        }
        else
        {
            try
            {
                await _connection.Auth.CurrentUser.UpdateEmailAsync(newEmail);
                success = true;
            }
            catch (Plugin.FirebaseAuth.FirebaseAuthException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AccountService] UpdateEmail: {ex.Message}");
                errorMessage = ex.Message;
            }
        }

        return (success, errorMessage);
    }

    /// <summary>Cambia la contraseña del usuario autenticado en Firebase Auth.</summary>
    public async Task<(bool Success, string? ErrorMessage)> ChangePassword(string newPassword)
    {
        bool success = false;
        string? errorMessage = null;

        if (_connection.Auth.CurrentUser == null)
        {
            errorMessage = "No hay sesión activa.";
        }
        else
        {
            try
            {
                await _connection.Auth.CurrentUser.UpdatePasswordAsync(newPassword);
                success = true;
            }
            catch (Plugin.FirebaseAuth.FirebaseAuthException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AccountService] ChangePassword: {ex.Message}");
                errorMessage = ex.Message;
            }
        }

        return (success, errorMessage);
    }

    /// <summary>Cierra la sesión del usuario y borra los datos de sesión persistidos.</summary>
    public void Logout()
    {
        _connection.Auth.SignOut();
        _currentUser = null;
        Preferences.Remove(SESSION_UID_KEY);
        Preferences.Remove(SESSION_EXPIRY_KEY);
    }

    /// <summary>
    /// Calcula y añade la experiencia ganada en una partida cerrada.
    /// Evita doble reclamación usando XpClaimedParties.
    /// </summary>
    public async Task ClaimPartyExperienceAsync(string roomCode, Party party)
    {
        if (_currentUser != null && party != null)
        {
            if (_currentUser.XpClaimedParties == null)
                _currentUser.XpClaimedParties = new List<string>();

            bool alreadyClaimed = _currentUser.XpClaimedParties.Contains(roomCode);

            if (!alreadyClaimed
                && party.PlayerScores != null
                && party.PlayerScores.TryGetValue(_currentUser.Id, out int scoreToAdd)
                && scoreToAdd > 0)
            {
                int newXp = _currentUser.Experience + scoreToAdd;

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

                if (_currentUser.GameHistory == null)
                    _currentUser.GameHistory = new List<string>();

                if (!_currentUser.GameHistory.Contains(roomCode))
                    _currentUser.GameHistory.Add(roomCode);

                await UpdateUser(_currentUser);
                System.Diagnostics.Debug.WriteLine($"[AccountService] XP reclamada para sala {roomCode}. +{scoreToAdd} XP -> Nivel {calculatedLevel}");
            }
        }
    }

    /// <summary>
    /// Revisa el historial del perfil del usuario al abrir el menú y reclama la XP
    /// de cualquier sala CLOSED de la que aún no hayamos cobrado.
    /// </summary>
    public async Task CheckAndClaimPendingExperienceAsync(ILobbyRepository lobbyRepo)
    {
        if (_currentUser != null)
        {
            // Refrescar usuario para tener GameHistory actualizado desde Firestore
            var freshUser = await _userRepo.GetUserAsync(_currentUser.Id);
            if (freshUser != null)
                _currentUser = freshUser;

            if (_currentUser.GameHistory != null && _currentUser.GameHistory.Count > 0)
            {
                if (_currentUser.XpClaimedParties == null)
                    _currentUser.XpClaimedParties = new List<string>();

                var pendingRooms = _currentUser.GameHistory.Except(_currentUser.XpClaimedParties).ToList();

                var partyTasks = pendingRooms.Select(code => lobbyRepo.GetPartyAsync(code));
                var parties = await Task.WhenAll(partyTasks);

                bool updated = false;
                string roomCode;
                Party? party;
                for (int i = 0; i < pendingRooms.Count; i++)
                {
                    roomCode = pendingRooms[i];
                    party = parties[i];

                    if (party != null && party.GameState == "CLOSED")
                    {
                        await ClaimPartyExperienceAsync(roomCode, party);
                        updated = true;
                    }
                }

                if (updated)
                {
                    var refreshedUser = await _userRepo.GetUserAsync(_currentUser.Id);
                    if (refreshedUser != null) _currentUser = refreshedUser;
                }
            }
        }
    }

    /// <summary>Devuelve el usuario actualmente autenticado en memoria, o null si no hay sesión.</summary>
    public User? GetCurrentUser() => _currentUser;
}