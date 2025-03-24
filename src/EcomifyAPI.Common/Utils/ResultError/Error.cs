namespace EcomifyAPI.Common.Utils.ResultError;

public sealed record Error : IError
{
    public string Description { get; init; }
    public string Code { get; init; }
    public ErrorType ErrorType { get; init; }

    private Error(string description, string code, ErrorType errorType)
    {
        Description = description;
        Code = code;
        ErrorType = errorType;
    }

    public static Error Failure(string description, string code)
        => new(description, code, ErrorType.Failure);

    public static Error Unexpected(string description, string code)
       => new(description, code, ErrorType.Unexpected);

    public static ValidationError Validation(string description, string code, string field)
      => ValidationError.Create(description, code, field);

    public static Error Conflict(string description, string code)
      => new(description, code, ErrorType.Conflict);

    public static Error NotFound(string description, string code)
      => new(description, code, ErrorType.NotFound);

    public static Error Unauthorized(string description, string code)
      => new(description, code, ErrorType.Unauthorized);
}