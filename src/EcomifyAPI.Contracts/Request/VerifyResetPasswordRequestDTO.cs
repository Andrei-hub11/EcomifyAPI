namespace EcomifyAPI.Contracts.Request;

public sealed record VerifyResetPasswordRequestDTO(string Token, string Email);