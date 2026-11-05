namespace RealTimeSystem.Application.Common.Exceptions
{
    public sealed class ValidationAppException : Exception
    {
        public ValidationAppException(IDictionary<string, string[]> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }

        public IDictionary<string, string[]> Errors { get; }
    }

    public sealed class UnauthorizedAppException : Exception
    {
        public UnauthorizedAppException(string message) : base(message)
        {
        }
    }
}

