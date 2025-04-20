namespace EcomifyAPI.Contracts.Response;

public sealed record AuthResponseDTO(
    UserResponseDTO User,
    IReadOnlySet<string> Roles);