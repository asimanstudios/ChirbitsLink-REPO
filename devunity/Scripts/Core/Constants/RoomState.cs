namespace ChibitsLink.Core.Constants
{
    /// <summary>
    /// Estados posibles de una sala de juego en Firestore.
    /// Utilizar estas constantes en lugar de cadenas literales en todo el proyecto.
    /// </summary>
    /// <remarks>
    /// Proporciona consistencia en los estados de sala.
    /// Facilita el mantenimiento y evita errores de tipeo.
    /// </remarks>
    public static class RoomState
    {
        /// <summary>Estado de sala en lobby esperando jugadores</summary>
        public const string Lobby = "LOBBY";
        /// <summary>Estado de sala en fase de votación</summary>
        public const string Voting = "VOTING";
        /// <summary>Estado de sala en juego activo</summary>
        public const string InGame = "IN_GAME";
        /// <summary>Estado de sala cerrada/finalizada</summary>
        public const string Closed = "CLOSED";
    }
}
