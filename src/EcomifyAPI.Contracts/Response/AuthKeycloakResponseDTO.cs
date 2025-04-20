namespace EcomifyAPI.Contracts.Response;

public sealed record AuthKeycloakResponseDTO(
    UserResponseDTO User,
    string AccessToken,
    string RefreshToken,
    IReadOnlySet<string> Roles
);