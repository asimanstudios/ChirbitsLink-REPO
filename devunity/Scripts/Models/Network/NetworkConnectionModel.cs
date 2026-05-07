using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ChibiCocina.Models
{
    /// <summary>
    /// Modelo de datos para conexión de red.
    /// Representa la información de un cliente conectado al servidor.
    /// </summary>
    /// <remarks>
    /// Incluye metadatos del jugador y timestamps de actividad.
    /// Utilizado para gestión de conexiones TCP y timeout.
    /// </remarks>
    public class NetworkConnectionModel
    {
        /// <summary>ID del usuario</summary>
        public string UserId { get; set; }
        /// <summary>Nombre del jugador</summary>
        public string Name { get; set; }
        /// <summary>ID del personaje seleccionado</summary>
        public string CharacterId { get; set; }
        /// <summary>Nivel del jugador</summary>
        public int Level { get; set; }
        /// <summary>Endpoint de la conexión</summary>
        public string EndPoint { get; set; }
        /// <summary>Timestamp de conexión</summary>
        public DateTime ConnectedAt { get; set; }
        /// <summary>Timestamp de última actividad</summary>
        public DateTime LastActivity { get; set; }
        /// <summary>Indica si la conexión está activa</summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Constructor por defecto.
        /// Inicializa timestamps y estado activo.
        /// </summary>
        public NetworkConnectionModel()
        {
            ConnectedAt = DateTime.Now;
            LastActivity = DateTime.Now;
            IsActive = true;
        }
        
        /// <summary>
        /// Actualiza el timestamp de última actividad.
        /// Utilizado para mantener conexiones vivas.
        /// </summary>
        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }
        
        /// <summary>
        /// Verifica si la conexión ha expirado por inactividad.
        /// </summary>
        /// <param name="timeout">Tiempo máximo de inactividad</param>
        /// <returns>True si la conexión ha expirado</returns>
        public bool IsExpired(TimeSpan timeout)
        {
            return DateTime.Now - LastActivity > timeout;
        }
    }
    
    /// <summary>
    /// Modelo de datos para sesión de red.
    /// Gestiona el estado de una sala de juego y sus conexiones.
    /// </summary>
    /// <remarks>
    /// Mantenía persistencia de datos de jugadores entre escenas.
    /// Proporciona métodos para gestión de conexiones.
    /// </remarks>
    public class NetworkSessionModel
    {
        /// <summary>Código de la sala</summary>
        public string RoomCode { get; set; }
        /// <summary>Conexiones activas por ID</summary>
        public Dictionary<string, NetworkConnectionModel> ActiveConnections { get; set; }
        /// <summary>Nombres de sesión por ID</summary>
        public Dictionary<string, string> SessionNames { get; set; }
        /// <summary>Personajes de sesión por ID</summary>
        public Dictionary<string, string> SessionCharacters { get; set; }
        /// <summary>Niveles de sesión por ID</summary>
        public Dictionary<string, int> SessionLevels { get; set; }
        /// <summary>Lista de IDs de jugadores activos</summary>
        public List<string> ActivePlayerIds { get; set; }
        /// <summary>Timestamp de creación de la sesión</summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Constructor por defecto.
        /// Inicializa diccionarios y listas.
        /// </summary>
        public NetworkSessionModel()
        {
            ActiveConnections = new Dictionary<string, NetworkConnectionModel>();
            SessionNames = new Dictionary<string, string>();
            SessionCharacters = new Dictionary<string, string>();
            SessionLevels = new Dictionary<string, int>();
            ActivePlayerIds = new List<string>();
            CreatedAt = DateTime.Now;
        }
        
        /// <summary>
        /// Añade una nueva conexión a la sesión.
        /// Actualiza todos los diccionarios de datos de jugador.
        /// </summary>
        /// <param name="connection">Conexión a añadir</param>
        public void AddConnection(NetworkConnectionModel connection)
        {
            ActiveConnections[connection.UserId] = connection;
            SessionNames[connection.UserId] = connection.Name;
            SessionCharacters[connection.UserId] = connection.CharacterId;
            SessionLevels[connection.UserId] = connection.Level;
            
            if (!ActivePlayerIds.Contains(connection.UserId))
            {
                ActivePlayerIds.Add(connection.UserId);
            }
        }
        
        /// <summary>
        /// Remueve una conexión de la sesión.
        /// Limpia datos del jugador de todos los diccionarios.
        /// </summary>
        /// <param name="userId">ID del usuario a remover</param>
        public void RemoveConnection(string userId)
        {
            if (ActiveConnections.ContainsKey(userId))
            {
                ActiveConnections.Remove(userId);
            }
            
            ActivePlayerIds.Remove(userId);
        }
        
        /// <summary>
        /// Obtiene una conexión por ID de usuario.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Conexión encontrada o null</returns>
        public NetworkConnectionModel GetConnection(string userId)
        {
            return ActiveConnections.ContainsKey(userId) ? ActiveConnections[userId] : null;
        }
        
        /// <summary>
        /// Obtiene el número de jugadores activos.
        /// </summary>
        /// <returns>Número de jugadores activos</returns>
        public int GetActivePlayerCount()
        {
            return ActivePlayerIds.Count;
        }
    }
}
