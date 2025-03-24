namespace EcomifyAPI.Contracts.Models;

public sealed record ValidationErrorDetails(
    string Type,
    string Title,
    int Status,
    string Detail,
    string Instance,
    Dictionary<string, ValidationErrorDetail[]> Errors
);