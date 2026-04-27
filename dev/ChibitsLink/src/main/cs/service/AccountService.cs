using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using ChibitsLink.main.cs.exception;
using ChibitsLink.main.cs.model;

namespace ChibitsLink.main.cs.service;

/// <summary>
/// Gestiona la autenticación de usuarios (inicio de sesión, registro, cierre de sesión)
/// y la persistencia de la sesión activa del usuario.
/// </summary>
public class AccountService : BaseService
{
    private readonly ChibitsLink.main.repository.Database _db;
    private readonly ChibitsLink.main.repository.FirebaseConnection _connection;
    private User? _currentUser;

    private const string SESSION_UID_KEY = "session_uid";
    private const string SESSION_EXPIRY_KEY = "session_expiry";

    public AccountService(ChibitsLink.main.repository.Database db, ChibitsLink.main.repository.FirebaseConnection connection)
    {
        _db = db;
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
                _currentUser = await _db.GetUser(userId);
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
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> Login(string email, string password)
    {
        try
        {
            var result = await _connection.Auth.SignInWithEmailAndPasswordAsync(email, password);
            if (result.User != null)
            {
                // FIX: Mayor pausa para asegurar propagación
                await Task.Delay(1500);

                _currentUser = await _db.GetUser(result.User.Uid);
                
                if (_currentUser == null)
                {
                    _connection.Auth.SignOut();
                    return (false, $"CRÍTICO: El perfil Firestore no existe para el UID Auth actual ({result.User.Uid}). Elimina tu cuenta desde la consola de Firebase Auth y regístrate de nuevo.");
                }

                _currentUser.Email = result.User.Email ?? email;
                await SaveSession(result.User.Uid);
                return (true, null);
            }
        }
        catch (DatabaseException ex)
        {
            return (false, $"Error al recuperar datos de usuario: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}");
            return (false, $"Error interno durante el login: {ex.Message}");
        }
        return (false, "La autenticación no devolvió un usuario.");
    }

    /// <summary>
    /// Registra un nuevo usuario en Firebase Auth y crea su perfil en Firestore.
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(string realName, string username, string email, string password)
    {
        try
        {
            var result = await _connection.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            if (result.User == null) return (false, "El registro no devolvió un usuario.");

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
                await _db.SaveUser(newUser);
            }
            catch (DatabaseException ex)
            {
                // ROLLBACK: Borrar la cuenta vacía de Auth si falla la escritura en BBDD para no dejarla "huérfana"
                await result.User.DeleteAsync();
                return (false, $"Error bloqueando en BBDD (probablemente Reglas/Tiempo). Cuenta de Auth cancelada para evitar bucles. Detalle: {ex.Message}");
            }
            _currentUser = newUser;
            await SaveSession(result.User.Uid);

            await _db.InitializeCharactersAsync();

            return (true, null);
        }
        catch (DatabaseException ex)
        {
            return (false, $"Error al guardar perfil de usuario: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Registration Error: {ex.Message}");
            return (false, "Error al crear la cuenta. Inténtalo de nuevo más tarde.");
        }
    }

    /// <summary>Actualiza los datos del usuario tanto en Firestore como en memoria.</summary>
    public async Task UpdateUser(User user)
    {
        await _db.UpdateUser(user);
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

    /// <summary>Devuelve el usuario actualmente autenticado en memoria, o null si no hay sesión.</summary>
    public User? GetCurrentUser() => _currentUser;
}