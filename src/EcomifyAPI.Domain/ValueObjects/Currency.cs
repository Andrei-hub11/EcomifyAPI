using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct Currency
{
    public string Code { get; init; } = string.Empty;
    public decimal Amount { get; init; }

    public Currency(string code, decimal amount)
    {
        var errors = ValidateCurrency(code, amount);

        if (errors.Count != 0)
        {
            throw new ArgumentException(string.Join(", ", errors.Select(e => e.Description)));
        }

        Code = code;
        Amount = amount;
    }

    private static ReadOnlyCollection<ValidationError> ValidateCurrency(string code, decimal amount)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(code))
        {
            errors.Add(ValidationError.Create("Currency code is required", "ERR_CURRENCY_CODE_REQUIRED", "CurrencyCode"));
        }

        if (amount <= 0)
        {
            errors.Add(ValidationError.Create("Amount must be greater than 0", "ERR_AMOUNT_MUST_BE_GREATER_THAN_0", "Amount"));
        }

        return errors.AsReadOnly();
    }
}