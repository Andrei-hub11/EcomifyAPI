using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct PayPalDetails : IPaymentMethodDetails
{
    public Email PayPalEmail { get; init; }
    public string PayPalPayerId { get; init; }

    public DateTime CreatedAt { get; init; }

    public PayPalDetails(string paypalEmail, string paypalPayerId)
    {
        if (string.IsNullOrWhiteSpace(paypalEmail))
            throw new DomainException(Error.Validation("Invalid PayPal email",
            "ERR_INVALID_PAYPAL_EMAIL", nameof(paypalEmail)));

        PayPalEmail = new Email(paypalEmail);
        PayPalPayerId = paypalPayerId;
        CreatedAt = DateTime.UtcNow;
    }
}