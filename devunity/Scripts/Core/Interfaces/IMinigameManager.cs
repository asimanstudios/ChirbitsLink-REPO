using System.Collections;

namespace Chirbits.Core
{
    public interface IMinigameManager
    {
        void StartGame();
        void EndGame();
        bool IsGameRunning { get; }
    }
}
