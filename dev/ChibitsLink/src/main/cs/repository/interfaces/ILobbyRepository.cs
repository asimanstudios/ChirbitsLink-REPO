using System;
using System.Threading.Tasks;
using ChibitsLink.main.cs.model;
using System.Collections.Generic;

namespace ChibitsLink.main.repository.interfaces;

public interface ILobbyRepository
{
    Task CreatePartyAsync(Party party);
    Task<Party?> GetPartyAsync(string roomCode);
    Task<bool> ExistsAsync(string roomCode);
    IDisposable ListenToParty(string roomCode, Action<Party?> onChanged);
    Task ToggleReadyAsync(string roomCode, string userId, bool isReady);
}
