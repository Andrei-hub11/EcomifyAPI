namespace EcomifyAPI.Contracts.Request;

public sealed record UserRegisterRequestDTO(
    string UserName,
    string Email,
    string Password,
    string ProfileImage);