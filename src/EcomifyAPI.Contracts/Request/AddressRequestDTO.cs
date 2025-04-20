namespace EcomifyAPI.Contracts.Request;

public sealed record AddressRequestDTO(
    string Street,
    int Number,
    string City,
    string State,
    string ZipCode,
    string Country,
    string Complement
);