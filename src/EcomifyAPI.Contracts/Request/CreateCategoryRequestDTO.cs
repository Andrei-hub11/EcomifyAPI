namespace EcomifyAPI.Contracts.Request;

public sealed record CreateCategoryRequestDTO(
    string Name,
    string Description
);