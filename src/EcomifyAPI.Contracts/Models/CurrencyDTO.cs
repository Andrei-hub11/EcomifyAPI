namespace EcomifyAPI.Contracts.Models;

public sealed record MoneyDTO(
    string Code,
    decimal Amount
);