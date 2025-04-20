namespace EcomifyAPI.Contracts.Models;

public sealed record AddressResponseDTO(
    Guid? Id,
    string Street,
    int Number,
    string City,
    string State,
    string ZipCode,
    string Country,
    string Complement
);