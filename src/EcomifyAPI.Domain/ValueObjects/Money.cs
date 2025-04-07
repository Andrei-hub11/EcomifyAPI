using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct Money
{
    public string Code { get; init; } = string.Empty;
    public decimal Amount { get; init; }

    public Money(string code, decimal amount)
    {
        var errors = ValidateCurrency(code, amount);

        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }

        Code = code;
        Amount = amount;
    }

    internal static Money Zero(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new DomainException(ValidationError.Create("Currency code is required", "ERR_CURRENCY_REQ", "CurrencyCode"));
        }

        return new Money { Code = currencyCode, Amount = 0 };
    }

    private static ReadOnlyCollection<ValidationError> ValidateCurrency(string code, decimal amount)
    {
        var errors = new List<ValidationError>();

        var validCurrencyCodes = new HashSet<string> { "BRL", "USD" };

        if (string.IsNullOrWhiteSpace(code))
        {
            errors.Add(ValidationError.Create("Currency code is required", "ERR_CURRENCY_REQ", "CurrencyCode"));
        }

        if (!validCurrencyCodes.Contains(code))
        {
            errors.Add(ValidationError.Create("Invalid currency code", "ERR_INVALID_CURRENCY", "CurrencyCode"));
        }

        if (amount <= 0)
        {
            errors.Add(ValidationError.Create("Amount must be greater than 0", "ERR_AMOUNT_GT_0", "Amount"));
        }

        return errors.AsReadOnly();
    }
}