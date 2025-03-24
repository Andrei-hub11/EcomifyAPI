namespace EcomifyAPI.Contracts.Response;

public sealed record UserResponseDTO(
    string Id,
    string UserName,
    string Email,
    string ProfileImagePath);