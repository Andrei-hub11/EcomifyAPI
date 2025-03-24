namespace EcomifyAPI.Contracts.EmailModels;

public sealed record PasswordResetEmail(string ResetLink, TimeSpan TokenValidity);