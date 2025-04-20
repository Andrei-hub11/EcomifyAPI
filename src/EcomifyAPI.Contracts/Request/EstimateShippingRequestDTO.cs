namespace EcomifyAPI.Contracts.Request;

public sealed record EstimateShippingRequestDTO(
    string ZipCode,
    string City,
    string State
);