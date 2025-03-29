using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

using EcomifyAPI.IntegrationTests.config;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using Npgsql;


using Respawn;
using Respawn.Graph;

using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;

namespace EcomifyAPI.IntegrationTests.Fixture;

public class AppHostFixture : IAsyncLifetime
{
    private readonly INetwork _network = new NetworkBuilder().Build();
    private PostgreSqlContainer _container = default!;
    private KeycloakContainer _keycloakContainer = default!;
    private const string Username = "postgres";
    private const string Password = "postgres";
    private const string Database = "testdb";
    private const int PostgreSqlPort = 5432;

    private KeycloakSetup _keycloakSetup = default!;
    private readonly string RealmName = "base-realm";
    private readonly string ClientId = "base-realm";

    private readonly string AdminUsername = "admin_11";
    private readonly string AdminPassword = "Adm1n_K3ycl0ak_2025!";
    private readonly string AdminEmail = "and122@gmail.com";

    private Respawner _respawner = default!;
    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    private static readonly string SetupPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Scripts",
        "init_script.sql"
    );

    public async Task InitializeAsync()
    {
        if (_container is not null)
        {
            return;
        }

        _container = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithEnvironment("POSTGRES_DB", Database)
            .WithEnvironment("POSTGRES_USER", Username)
            .WithEnvironment("POSTGRES_PASSWORD", Password)
            .WithPortBinding(PostgreSqlPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .WithNetwork(_network)
            .WithNetworkAliases("postgres")
            .Build();

        _keycloakContainer = new KeycloakBuilder()
            .WithImage("quay.io/keycloak/keycloak:latest")
            .WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL_HOST", "postgres")
            .WithEnvironment("KC_DB_URL_DATABASE", Database)
            .WithEnvironment("KC_DB_USERNAME", Username)
            .WithEnvironment("KC_DB_PASSWORD", Password)
            .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", AdminUsername)
            .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", AdminPassword)
            .WithPortBinding(9090, true)
            .WithNetwork(_network)
            .WithResourceMapping(new FileInfo("realm-export.json"),
            new FileInfo("/opt/keycloak/data/import/realm-export.json"))
            .WithCommand("--import-realm")
            .Build();

        await _container.StartAsync();
        await _keycloakContainer.StartAsync();
        _keycloakSetup = new KeycloakSetup(_keycloakContainer, RealmName, ClientId, AdminUsername, AdminPassword, AdminEmail);
        await _keycloakSetup.SetupUserAndGroup();
        var clientSecret = await _keycloakSetup.GetClientSecret();
        var sqlFilePath = SetupPath;

        if (!File.Exists(sqlFilePath))
        {
            throw new FileNotFoundException("SQL initialization script not found.", sqlFilePath);
        }

        await InitializeDatabaseAsync(GetConnectionString(), SetupPath);

        using (var connection = new NpgsqlConnection(GetConnectionString()))
        {
            await connection.OpenAsync();
            _respawner = await Respawner.CreateAsync(connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore =
    [
            new Table("admin_event_entity"),
            new Table("associated_policy"),
            new Table("authentication_execution"),
            new Table("authentication_flow"),
            new Table("authenticator_config"),
            new Table("authenticator_config_entry"),
            new Table("broker_link"),
            new Table("client"),
            new Table("client_attributes"),
            new Table("client_auth_flow_bindings"),
            new Table("client_authenticator"),
            new Table("client_initial_access"),
            new Table("client_node_registrations"),
            new Table("client_scope"),
            new Table("client_scope_attributes"),
            new Table("client_scope_client"),
            new Table("client_scope_role_mapping"),
            new Table("client_session"),
            new Table("client_user_session_note"),
            new Table("component"),
            new Table("component_config"),
            new Table("composite_role"),
            new Table("credential"),
            new Table("databasechangelog"),
            new Table("databasechangeloglock"),
            new Table("default_client_scope"),
            new Table("event_entity"),
            new Table("fed_user_attribute"),
            new Table("fed_user_consent"),
            new Table("fed_user_consent_cl_scope"),
            new Table("fed_user_credential"),
            new Table("fed_user_group_membership"),
            new Table("fed_user_required_action"),
            new Table("fed_user_role_mapping"),
            new Table("federated_identity"),
            new Table("federated_user"),
            new Table("group_attribute"),
            new Table("group_role_mapping"),
            new Table("identity_provider"),
            new Table("identity_provider_config"),
            new Table("identity_provider_mapper"),
            new Table("idp_mapper_config"),
            new Table("jgroups_ping"),
            new Table("keycloak_group"),
            new Table("keycloak_role"),
            new Table("migration_model"),
            new Table("offline_client_session"),
            new Table("offline_user_session"),
            new Table("org"),
            new Table("org_domain"),
            new Table("policy_config"),
            new Table("protocol_mapper"),
            new Table("protocol_mapper_config"),
            new Table("realm"),
            new Table("realm_attribute"),
            new Table("realm_default_groups"),
            new Table("realm_enabled_event_types"),
            new Table("realm_events_listeners"),
            new Table("realm_localizations"),
            new Table("realm_required_credential"),
            new Table("realm_smtp_config"),
            new Table("realm_supported_locales"),
            new Table("redirect_uri"),
            new Table("required_action_config"),
            new Table("required_action_provider"),
            new Table("resource_attribute"),
            new Table("resource_server"),
            new Table("resource_server_perm_ticket"),
            new Table("resource_server_scope"),
            new Table("resource_server_policy"),
            new Table("resource_server_resource"),
            new Table("resource_uris"),
            new Table("revoked_token"),
            new Table("role_attribute"),
            new Table("scope_mapping"),
            new Table("scope_policy"),
            new Table("user_attribute"),
            new Table("user_consent"),
            new Table("user_consent_client_scope"),
            new Table("user_entity"),
            new Table("user_federation_config"),
            new Table("user_federation_mapper"),
            new Table("user_federation_mapper_config"),
            new Table("user_federation_provider"),
            new Table("user_group_membership"),
            new Table("user_required_action"),
            new Table("user_role_mapping"),
            new Table("user_session"),
            new Table("user_session_note"),
            new Table("web_origins")
    ]
            });
        }

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseContentRoot(GetProjectPath());
            builder.ConfigureAppConfiguration(
                (context, config) =>
                {
                    var builtConfig = config.Build();

                    if (string.IsNullOrWhiteSpace(GetConnectionString()))
                    {
                        throw new InvalidOperationException(
                            "The database connection cannot be empty."
                        );
                    }

                    config.AddInMemoryCollection(
                        new List<KeyValuePair<string, string?>>
                        {
                            new KeyValuePair<string, string?>(
                                "ConnectionStrings:DefaultConnection",
                                GetConnectionString()
                            ),
                            new("UserKeycloakAdmin:grant_type", "password"),
                            new("UserKeycloakAdmin:client_id", ClientId),
                            new("UserKeycloakAdmin:username", "admin_11"),
                            new("UserKeycloakAdmin:password", "Adm1n_K3ycl0ak_2025!"),
                            new("UserKeycloakAdmin:client_secret", clientSecret),
                            new("UserKeycloakAdmin:TokenEndpoint", $"{_keycloakContainer.GetBaseAddress()}/realms/base-realm/protocol/openid-connect/token"),
                            new("UserKeycloakAdmin:EndpointBase", $"{_keycloakContainer.GetBaseAddress()}/admin/realms/base-realm"),
                            new("UserKeycloakClient:grant_type", "password"),
                            new("UserKeycloakClient:client_id", ClientId),
                            new("UserKeycloakClient:client_secret", clientSecret),
                            new("UserKeycloakClient:TokenEndpoint", $"{_keycloakContainer.GetBaseAddress()}/realms/base-realm/protocol/openid-connect/token"),
                            new("UserKeycloakClient:EndpointBase", $"{_keycloakContainer.GetBaseAddress()}/realms/base-realm")
                        }
                    );
                }
            );
        });

        _client = _factory.CreateClient();
    }

    public HttpClient CreateClient() => _client;

    public string GetConnectionString()
    {
        return $"Host=localhost;Port={_container.GetMappedPublicPort(5432)};Database={Database};Username={Username};Password={Password};";
    }

    public async Task IsPostgresReady()
    {
        var connection = new NpgsqlConnection(GetConnectionString());
        await connection.OpenAsync();
        await connection.CloseAsync();
    }

    private static string GetProjectPath()
    {
        var projectName = "EcomifyAPI.Api";
        var baseDirectory = AppContext.BaseDirectory;

        var rootDirectory = new DirectoryInfo(baseDirectory);
        while (rootDirectory is not null && rootDirectory.GetDirectories("src").Length == 0)
        {
            rootDirectory = rootDirectory.Parent;
        }

        if (rootDirectory is null)
        {
            throw new DirectoryNotFoundException(
                "Root directory containing 'src' folder not found."
            );
        }

        var projectDirectory = rootDirectory
            .GetDirectories("src")[0]
            .GetDirectories()
            .FirstOrDefault(d => d.Name == projectName);

        if (projectDirectory is null)
        {
            throw new DirectoryNotFoundException(
                $"The project '{projectName}' was not found under 'src'."
            );
        }

        return projectDirectory.FullName;
    }

    private async Task InitializeDatabaseAsync(string connectionString, string sqlFilePath)
    {
        var sql = await File.ReadAllTextAsync(sqlFilePath);

        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task ResetAsync()
    {
        using var connection = new NpgsqlConnection(GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await _keycloakContainer.DisposeAsync();
        await _network.DisposeAsync();
    }
}