using System;

namespace ChibitsLink.Core.Exceptions
{
    #region Base Repository Exceptions
    
    public class RepositoryException : Exception
    {
        public RepositoryException(string message) : base(message) { }
        public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class RepositoryNotInitializedException : RepositoryException
    {
        public RepositoryNotInitializedException(string message) : base(message) { }
        public RepositoryNotInitializedException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion

    #region Party Repository Exceptions

    public class PartyException : RepositoryException
    {
        public PartyException(string message) : base(message) { }
        public PartyException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidPartyException : PartyException
    {
        public InvalidPartyException(string message) : base(message) { }
        public InvalidPartyException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PartyNotFoundException : PartyException
    {
        public PartyNotFoundException(string message) : base(message) { }
        public PartyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PartyCreationException : PartyException
    {
        public PartyCreationException(string message) : base(message) { }
        public PartyCreationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PartyRetrievalException : PartyException
    {
        public PartyRetrievalException(string message) : base(message) { }
        public PartyRetrievalException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PartyUpdateException : PartyException
    {
        public PartyUpdateException(string message) : base(message) { }
        public PartyUpdateException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PartyDeletionException : PartyException
    {
        public PartyDeletionException(string message) : base(message) { }
        public PartyDeletionException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion

    #region User Repository Exceptions

    public class UserException : RepositoryException
    {
        public UserException(string message) : base(message) { }
        public UserException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidUserException : UserException
    {
        public InvalidUserException(string message) : base(message) { }
        public InvalidUserException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UserNotFoundException : UserException
    {
        public UserNotFoundException(string message) : base(message) { }
        public UserNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UserCreationException : UserException
    {
        public UserCreationException(string message) : base(message) { }
        public UserCreationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UserRetrievalException : UserException
    {
        public UserRetrievalException(string message) : base(message) { }
        public UserRetrievalException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UserUpdateException : UserException
    {
        public UserUpdateException(string message) : base(message) { }
        public UserUpdateException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class UserDeletionException : UserException
    {
        public UserDeletionException(string message) : base(message) { }
        public UserDeletionException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion

    #region Session Repository Exceptions

    public class SessionException : RepositoryException
    {
        public SessionException(string message) : base(message) { }
        public SessionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidSessionException : SessionException
    {
        public InvalidSessionException(string message) : base(message) { }
        public InvalidSessionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SessionNotFoundException : SessionException
    {
        public SessionNotFoundException(string message) : base(message) { }
        public SessionNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SessionCreationException : SessionException
    {
        public SessionCreationException(string message) : base(message) { }
        public SessionCreationException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SessionRetrievalException : SessionException
    {
        public SessionRetrievalException(string message) : base(message) { }
        public SessionRetrievalException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SessionUpdateException : SessionException
    {
        public SessionUpdateException(string message) : base(message) { }
        public SessionUpdateException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SessionDeletionException : SessionException
    {
        public SessionDeletionException(string message) : base(message) { }
        public SessionDeletionException(string message, Exception innerException) : base(message, innerException) { }
    }

    #endregion
}
