using System;

namespace ChibiCocina.Core.Exceptions
{
    public class SceneLoaderException : Exception
    {
        public SceneLoaderException(string message) : base(message) { }
        public SceneLoaderException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class BotServiceException : Exception
    {
        public BotServiceException(string message) : base(message) { }
        public BotServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class DebugServiceException : Exception
    {
        public DebugServiceException(string message) : base(message) { }
        public DebugServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
