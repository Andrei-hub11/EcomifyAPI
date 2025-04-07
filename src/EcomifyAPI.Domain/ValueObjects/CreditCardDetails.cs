
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public class CreditCardDetails : IPaymentMethodDetails
{
    public string LastFourDigits { get; init; }
    public string CardBrand { get; init; }

    public DateTime CreatedAt { get; init; }

    public CreditCardDetails(string lastFourDigits, string cardBrand)
    {
        if (string.IsNullOrWhiteSpace(lastFourDigits) || lastFourDigits.Length != 4)
            throw new DomainException(Error.Validation("Invalid last four digits of the card",
            "ERR_INVALID_CARD", nameof(lastFourDigits)));

        LastFourDigits = lastFourDigits;
        CardBrand = cardBrand;
        CreatedAt = DateTime.UtcNow;
    }
}