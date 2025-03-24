namespace EcomifyAPI.Domain.Exceptions;

public class UnauthorizeUserAccessException : Exception
{
    public UnauthorizeUserAccessException(string message) : base(message)
    { }
}