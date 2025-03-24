namespace EcomifyAPI.Contracts.Models;

public sealed record CurrencyDTO(
    string Code,
    decimal Amount
);