namespace EcomifyAPI.Contracts.DapperModels;

public class ApplicationUserMapping
{
    public Guid Id { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfileImagePath { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}