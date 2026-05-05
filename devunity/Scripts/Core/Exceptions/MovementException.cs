using System;

namespace ChibiCocina.Core.Exceptions
{
    public class MovementException : Exception
    {
        public MovementException(string message) : base(message) { }
        public MovementException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class InvalidMovementStateException : MovementException
    {
        public InvalidMovementStateException(string state) : base($"Estado de movimiento inválido: {state}") { }
    }
    
    public class ComponentNotFoundException : MovementException
    {
        public ComponentNotFoundException(string componentName) : base($"Componente no encontrado: {componentName}") { }
        public ComponentNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class AudioServiceException : MovementException
    {
        public AudioServiceException(string audioClipName) : base($"Error al reproducir audio: {audioClipName}") { }
    }
}
