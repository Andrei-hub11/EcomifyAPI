using System.Diagnostics.CodeAnalysis;

using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Utils.Result;

public class Result
{
    protected Result(IReadOnlyList<IError> error)
    {
        Errors = error ?? [];
    }

    public IReadOnlyList<IError> Errors;

    public static Result<List<IError>> Fail(string errorMessage)
    {
        return new Result<List<IError>>(
            [],
            true,
            [ErrorFactory.Failure(errorMessage)]
        );
    }

    public static Result<List<IError>> Fail(IError error)
    {
        return new Result<List<IError>>([], true, [error]);
    }

    public static Result<List<IError>> Fail(List<IError> errors)
    {
        return new Result<List<IError>>([], true, errors);
    }

    public static Result<List<IError>> Fail(IReadOnlyList<IError> errors)
    {
        return new Result<List<IError>>([], true, errors);
    }

    public static Result<List<IError>> Fail(IReadOnlyList<ValidationError> errors)
    {
        return new Result<List<IError>>([], true, errors);
    }

    public static Result<T> Ok<T>(T value) => new(value, false, []);
}

public partial class Result<T> : Result
{
    public T? Value { get; }

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure { get; }
    public IError? Error
    {
        get
        {
            return !IsFailure ? ErrorFactory.Failure("Não há nenhum Error.") : Errors[0];
        }
    }

    protected internal Result(T? value, bool isFail, IReadOnlyList<IError> errors)
        : base(errors)
    {
        Value = value;
        IsFailure = isFail;
    }

    public static implicit operator Result<T>(T value) =>
        new(value, false, []);

    public static implicit operator Result<T>(Error error) =>
        new(default, true, [error]);

    /// <summary>
    /// Converte implicitamente um resultado contendo uma lista de erros em um resultado de um tipo especificado.
    /// </summary>
    /// <typeparam name="T">O tipo de valor retornado no resultado.</typeparam>
    /// <param name="errorResult">O resultado contendo uma lista de erros.</param>

    public static implicit operator Result<T>(Result<List<IError>> errorResult)
    {
        return new Result<T>(default, true, [.. errorResult.Errors]);
    }
}