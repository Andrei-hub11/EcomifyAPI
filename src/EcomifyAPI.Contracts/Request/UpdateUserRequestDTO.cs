namespace EcomifyAPI.Contracts.Request;

public sealed record UpdateUserRequestDTO(
    string NewUserName,
    string NewEmail,
    string NewPassword,
    string NewProfileImage
  );