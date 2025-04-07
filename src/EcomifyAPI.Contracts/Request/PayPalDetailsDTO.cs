using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Request;

public sealed record PayPalDetailsDTO(
    Guid PayerId,
    string PayerEmail
) : PaymentDetails;