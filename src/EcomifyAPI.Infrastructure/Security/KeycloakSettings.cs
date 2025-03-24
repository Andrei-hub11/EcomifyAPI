namespace EcomifyAPI.Infrastructure.Security;

public class KeycloakSettings
{
    public string Realm { get; set; } = string.Empty;
    public string AuthServerUrl { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public bool VerifyTokenAudience { get; set; }
    public Credentials Credentials { get; set; } = default!;
    public int ConfidentialPort { get; set; }
    public PolicyEnforcer PolicyEnforcer { get; set; } = default!;
}

public class Credentials
{
    public string Secret { get; set; } = string.Empty;
}

public class PolicyEnforcer
{
    public Credentials Credentials { get; set; } = default!;
}