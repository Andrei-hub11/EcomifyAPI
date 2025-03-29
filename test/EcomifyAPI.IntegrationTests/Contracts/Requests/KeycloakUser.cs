namespace EcomifyAPI.IntegrationTests.Contracts.Requests;

public class KeycloakUser
{
    public string username { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public bool enabled { get; set; } = true;
    public Credential[] credentials { get; set; } = Array.Empty<Credential>();
}

public class Credential
{
    public string type { get; set; } = string.Empty;
    public string value { get; set; } = string.Empty;
    public bool temporary { get; set; } = false;
}