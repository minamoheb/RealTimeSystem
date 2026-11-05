namespace RealTimeSystem.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }

    public sealed class NotFoundException : DomainException
    {
        public NotFoundException(string resource, object key)
            : base($"{resource} '{key}' was not found.")
        {
        }
    }

    public sealed class ForbiddenException : DomainException
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }

    public sealed class ConflictException : DomainException
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}

