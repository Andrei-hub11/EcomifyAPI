namespace EcomifyAPI.Contracts.Request;

public sealed record UpdatePasswordRequestDTO(string NewPassword, string UserId, string Token);