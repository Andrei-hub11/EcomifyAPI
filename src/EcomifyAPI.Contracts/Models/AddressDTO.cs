namespace EcomifyAPI.Contracts.Models;

public sealed record AddressDTO(
    string Street,
    int Number,
    string City,
    string State,
    string ZipCode,
    string Country,
    string Complement
);