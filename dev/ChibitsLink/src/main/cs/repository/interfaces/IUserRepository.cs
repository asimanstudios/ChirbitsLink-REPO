using System.Threading.Tasks;
using ChibitsLink.main.cs.model;
using System.Collections.Generic;

namespace ChibitsLink.main.repository.interfaces;

public interface IUserRepository
{
    Task<User?> GetUserAsync(string id);
    Task SaveUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task AddToHistoryAsync(string userId, string roomCode);
    Task<List<Party>> GetUserHistoryAsync(string userId);
}
