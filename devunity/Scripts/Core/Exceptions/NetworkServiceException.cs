using System;

namespace ChibiCocina.Core.Exceptions
{
    public class NetworkServiceException : Exception
    {
        public NetworkServiceException(string message) : base(message) { }
        public NetworkServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class TcpServerException : Exception
    {
        public TcpServerException(string message) : base(message) { }
        public TcpServerException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class ConnectionException : NetworkServiceException
    {
        public ConnectionException(string message) : base(message) { }
        public ConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }
    
    public class MessageProcessingException : NetworkServiceException
    {
        public MessageProcessingException(string message) : base(message) { }
        public MessageProcessingException(string message, Exception innerException) : base(message, innerException) { }
    }
}
