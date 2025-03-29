using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Domain.Exceptions;

public sealed class DomainException : Exception
{
    public IReadOnlyList<IError> Errors { get; }
    public DomainException(string message) : base(message)
    {
        Errors = [Error.Failure(message, "ERR_DOMAIN_EXCEPTION")];
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = [Error.Failure(message, "ERR_DOMAIN_EXCEPTION")];
    }

    public DomainException(IReadOnlyList<IError> errors)
    {
        Errors = errors;
    }

    public DomainException(IError error)
    {
        Errors = [error];
    }
}