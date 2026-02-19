namespace ChibitsLink.main.cs.service;

using System.Threading.Tasks;
using ChibitsLink.main.cs.model;
using Microsoft.Maui.Storage;

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

    public async Task<bool> IsSessionActiveAsync()
    {
        try
        {
            var userId = await SecureStorage.GetAsync(SESSION_UID_KEY);
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
        await SecureStorage.SetAsync(SESSION_UID_KEY, uid);
        Preferences.Set(SESSION_EXPIRY_KEY, DateTime.Now.AddDays(30).ToString());
    }



    public async Task<(bool Success, string? ErrorMessage)> Login(string email, string password)
    {
        try
        {
            var result = await _connection.Auth.SignInWithEmailAndPasswordAsync(email, password);
            if (result.User != null)
            {
                _currentUser = await _db.GetUser(result.User.Uid);
                await SaveSession(result.User.Uid);
                return (true, null);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login Error: {ex.Message}");
            return (false, ex.Message);
        }
        return (false, "Login failed");
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(string realName, string username, string email, string password)
    {
        try
        {
            var result = await _connection.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            if (result.User == null) return (false, "Registration response was null");

            var newUser = new User 
            { 
                Id = result.User.Uid,
                RealName = realName,
                Username = username 
            };
            
            await _db.SaveUser(newUser);
            _currentUser = newUser;
            await SaveSession(result.User.Uid);
            
            // Initialization of characters if first user
            await _db.InitializeCharactersAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Registration Error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task UpdateUser(User user)
    {
        await _db.UpdateUser(user);
        _currentUser = user;
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateEmail(string newEmail)
    {
        try
        {
            await _connection.Auth.CurrentUser.UpdateEmailAsync(newEmail);
            return (true, null);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ChangePassword(string newPassword)
    {
        try
        {
            await _connection.Auth.CurrentUser.UpdatePasswordAsync(newPassword);
            return (true, null);
        }
        catch (Exception e)
        {
            return (false, e.Message);
        }
    }

    public async Task Logout()
    {
        _connection.Auth.SignOut();
        _currentUser = null;
        SecureStorage.Remove(SESSION_UID_KEY);
        Preferences.Remove(SESSION_EXPIRY_KEY);
        await Task.CompletedTask;
    }

    public User? GetCurrentUser() => _currentUser;
}