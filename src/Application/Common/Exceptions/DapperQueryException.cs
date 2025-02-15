namespace Application.Common.Exceptions;

public class DapperQueryException : Exception
{
    public DapperQueryException(string message, Exception innerException)
        : base(message, innerException) { }
}
