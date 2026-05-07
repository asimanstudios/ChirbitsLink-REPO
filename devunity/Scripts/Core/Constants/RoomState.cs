namespace ChibitsLink.Core.Constants
{
    /// <summary>
    /// Possible states of a game room in Firestore.
    /// Use these constants instead of literal strings throughout the project.
    /// </summary>
    public static class RoomState
    {
        public const string Lobby = "LOBBY";
        public const string Voting = "VOTING";
        public const string InGame = "IN_GAME";
        public const string Closed = "CLOSED";
    }
}
