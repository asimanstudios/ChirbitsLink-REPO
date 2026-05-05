namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Estados posibles de una sala de juego en Firestore.
    /// Usar estas constantes en lugar de strings literales en todo el proyecto.
    /// </summary>
    public static class GameState
    {
        public const string Lobby  = "LOBBY";
        public const string Voting = "VOTING";
        public const string InGame = "IN_GAME";
        public const string Closed = "CLOSED";
    }
}
