namespace EcomifyAPI.Common.Utils.ResultError;

public record ValidationError : IError
{
    public string Description { get; init; }
    public string Code { get; init; }
    public string Field { get; init; }
    public ErrorType ErrorType { get; init; }

    private ValidationError(string description, string code, string field)
    {
        Description = description;
        Code = code;
        Field = field;
        ErrorType = ErrorType.Validation;
    }

    public static ValidationError Create(string description, string code, string field)
    {
        return new ValidationError(description, code, field);
    }
}