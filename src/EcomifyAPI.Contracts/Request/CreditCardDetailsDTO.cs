using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Request;

public sealed record CreditCardDetailsDTO(
    string CardNumber,
    string CardholderName,
    string ExpiryDate,
    string Cvv
) : PaymentDetails;