using System;

namespace Chirbits.Core.Exceptions
{
    public class ChirbitsGameException : Exception
    {
        public ChirbitsGameException(string message) : base(message) { }
        public ChirbitsGameException(string message, Exception inner) : base(message, inner) { }
    }

    public class FirestoreSyncException : ChirbitsGameException
    {
        public string Collection { get; }
        public FirestoreSyncException(string message, string collection) : base(message)
        {
            Collection = collection;
        }
    }

    public class SocketProtocolException : ChirbitsGameException
    {
        public string RawMessage { get; }
        public SocketProtocolException(string message, string rawMessage) : base(message)
        {
            RawMessage = rawMessage;
        }
    }

    public class SessionLogicException : ChirbitsGameException
    {
        public SessionLogicException(string message) : base(message) { }
    }
}
