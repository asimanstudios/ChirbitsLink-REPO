using System;
using System.Text;

namespace ChibiCocina.Models
{
    /// <summary>
    /// Modelo de datos para mensajes de red.
    /// Representa un mensaje enviado entre cliente y servidor.
    /// </summary>
    /// <remarks>
    /// Incluye metadatos como prioridad y timestamp.
    /// Soporta serialización JSON básica.
    /// Utilizado para comunicación TCP.
    /// </remarks>
    public class NetworkMessageModel
    {
        /// <summary>Tipo del mensaje</summary>
        public string Type { get; set; }
        /// <summary>ID del usuario emisor</summary>
        public string UserId { get; set; }
        /// <summary>Contenido del mensaje</summary>
        public string Content { get; set; }
        /// <summary>Timestamp del mensaje</summary>
        public DateTime Timestamp { get; set; }
        /// <summary>Prioridad del mensaje</summary>
        public MessagePriority Priority { get; set; }
        /// <summary>Indica si requiere respuesta</summary>
        public bool RequiresResponse { get; set; }
        
        /// <summary>
        /// Constructor por defecto.
        /// Inicializa timestamp y prioridad normal.
        /// </summary>
        public NetworkMessageModel()
        {
            Timestamp = DateTime.Now;
            Priority = MessagePriority.Normal;
        }
        
        /// <summary>
        /// Convierte el mensaje a formato JSON.
        /// Implementación básica sin dependencias externas.
        /// </summary>
        /// <returns>String JSON del mensaje</returns>
        public string ToJson()
        {
            return $@"{{""type"":""{Type}"",""userId"":""{UserId}"",""content"":""{Content}"",""timestamp"":""{Timestamp:yyyy-MM-dd HH:mm:ss}""}}";
        }
        
        /// <summary>
        /// Parsea un mensaje desde formato JSON.
        /// Implementación básica de parsing manual.
        /// </summary>
        /// <param name="json">String JSON a parsear</param>
        /// <returns>Mensaje parseado</returns>
        public static NetworkMessageModel FromJson(string json)
        {
            // Implementación básica de parsing JSON
            var message = new NetworkMessageModel();
            
            if (json.Contains("\"type\":"))
            {
                var typeStart = json.IndexOf("\"type\":\"") + 8;
                var typeEnd = json.IndexOf("\"", typeStart);
                message.Type = json.Substring(typeStart, typeEnd - typeStart);
            }
            
            if (json.Contains("\"userId\":"))
            {
                var userIdStart = json.IndexOf("\"userId\":\"") + 10;
                var userIdEnd = json.IndexOf("\"", userIdStart);
                message.UserId = json.Substring(userIdStart, userIdEnd - userIdStart);
            }
            
            if (json.Contains("\"content\":"))
            {
                var contentStart = json.IndexOf("\"content\":\"") + 11;
                var contentEnd = json.IndexOf("\"", contentStart);
                message.Content = json.Substring(contentStart, contentEnd - contentStart);
            }
            
            return message;
        }
        
        /// <summary>
        /// Verifica si el mensaje es válido.
        /// Requiere tipo y ID de usuario.
        /// </summary>
        /// <returns>True si el mensaje es válido</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(UserId);
        }
    }
    
    /// <summary>
    /// Modelo de datos para mensajes de input de controlador.
    /// Representa input de jugadores desde dispositivos móviles.
    /// </summary>
    /// <remarks>
    /// Soporta diferentes tipos: botones, joysticks, sensores.
    /// Incluye coordenadas y valores numéricos.
    /// </remarks>
    public class ControllerInputMessage
    {
        /// <summary>Tipo de input</summary>
        public string Type { get; set; }
        /// <summary>ID del input</summary>
        public string Id { get; set; }
        /// <summary>Estado del input</summary>
        public string State { get; set; }
        /// <summary>Coordenada X</summary>
        public float X { get; set; }
        /// <summary>Coordenada Y</summary>
        public float Y { get; set; }
        /// <summary>ID del usuario</summary>
        public string UserId { get; set; }
        /// <summary>Tipo de sensor</summary>
        public string Sensor { get; set; }
        /// <summary>Valor numérico</summary>
        public float Value { get; set; }
        /// <summary>Timestamp del input</summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Constructor por defecto.
        /// Inicializa timestamp actual.
        /// </summary>
        public ControllerInputMessage()
        {
            Timestamp = DateTime.Now;
        }
        
        /// <summary>
        /// Verifica si el input es de tipo botón.
        /// </summary>
        /// <returns>True si es input de botón o touch</returns>
        public bool IsButtonInput()
        {
            return !string.IsNullOrEmpty(Type) && (Type == "button" || Type == "touch");
        }
        
        /// <summary>
        /// Verifica si el input es de tipo joystick.
        /// </summary>
        /// <returns>True si es input de joystick</returns>
        public bool IsJoystickInput()
        {
            return !string.IsNullOrEmpty(Type) && Type == "joystick";
        }
        
        /// <summary>
        /// Verifica si el input es de tipo sensor.
        /// </summary>
        /// <returns>True si es input de sensor</returns>
        public bool IsSensorInput()
        {
            return !string.IsNullOrEmpty(Sensor);
        }
        
        /// <summary>
        /// Verifica si el mensaje de input es válido.
        /// Requiere tipo y ID de usuario.
        /// </summary>
        /// <returns>True si el input es válido</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(Type);
        }
    }
    
    /// <summary>
    /// Prioridades para mensajes de red.
    /// Define la importancia y orden de procesamiento.
    /// </summary>
    public enum MessagePriority
    {
        /// <summary>Prioridad baja</summary>
        Low = 0,
        /// <summary>Prioridad normal</summary>
        Normal = 1,
        /// <summary>Prioridad alta</summary>
        High = 2,
        /// <summary>Prioridad crítica</summary>
        Critical = 3
    }
    
    /// <summary>
    /// Tipos de mensajes de red.
    /// Clasifica los mensajes por su propósito.
    /// </summary>
    public enum MessageType
    {
        /// <summary>Tipo desconocido</summary>
        Unknown,
        /// <summary>Mensaje de conexión</summary>
        Connection,
        /// <summary>Mensaje de desconexión</summary>
        Disconnection,
        /// <summary>Input de controlador</summary>
        ControllerInput,
        /// <summary>Mensaje de sincronización</summary>
        Sync,
        /// <summary>Mensaje de listo</summary>
        Ready,
        /// <summary>Mensaje de votación</summary>
        Vote,
        /// <summary>Estado del juego</summary>
        GameState,
        /// <summary>Mensaje de chat</summary>
        Chat,
        /// <summary>Mensaje de sistema</summary>
        System
    }
}
