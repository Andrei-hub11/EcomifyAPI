using EcomifyAPI.Common.Helpers;
using EcomifyAPI.IntegrationTests.Api;
using EcomifyAPI.IntegrationTests.Contracts.Requests;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Refit;

using Testcontainers.Keycloak;

namespace EcomifyAPI.IntegrationTests.config;

public class KeycloakSetup
{
    private readonly KeycloakContainer _keycloakContainer;
    private readonly string _realmName;
    private readonly string _clientId;
    private readonly string _adminUsername;
    private readonly string _adminPassword;
    private readonly string _adminEmail;

    public KeycloakSetup(
        KeycloakContainer keycloakContainer,
        string realmName,
        string clientId,
        string adminUsername,
        string adminPassword,
        string adminEmail)
    {
        _keycloakContainer = keycloakContainer;
        _realmName = realmName;
        _clientId = clientId;
        _adminUsername = adminUsername;
        _adminPassword = adminPassword;
        _adminEmail = adminEmail;
    }

    private RefitSettings GetRefitSettings()
    {
        return new RefitSettings
        {
            ContentSerializer = new NewtonsoftJsonContentSerializer(
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                }
            )
        };
    }

    public async Task<string> GetAdminToken()
    {
        var payload = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", "admin-cli" },
            { "username", _adminUsername },
            { "password", _adminPassword },
        };

        var baseRealm = _keycloakContainer.GetBaseAddress();
        var newClient = RestService.For<IKeycloakClientApi>(baseRealm, GetRefitSettings());

        var response = await newClient.GetToken(payload);

        ThrowHelper.ThrowIfNull(response.AccessToken);
        return response.AccessToken;
    }

    public async Task SetupUserAndGroup()
    {
        var adminToken = await GetAdminToken();
        var baseRealm = _keycloakContainer.GetBaseAddress();
        var newClient = RestService.For<IKeycloakClientApi>(baseRealm, GetRefitSettings());

        var newUser = new KeycloakUser
        {
            username = _adminUsername,
            email = _adminEmail,
            enabled = true,
            credentials = [new Credential { type = "password", value = _adminPassword, temporary = false }]
        };

        // Verifica se o usuário já existe
        var users = await newClient.GetUsers(_realmName, $"Bearer {adminToken}");
        var userExists = users.FirstOrDefault(u => u.Email == _adminEmail);

        if (userExists is not null)
        {
            return;
        }

        var response = await newClient.CreateUser(newUser, _realmName, $"Bearer {adminToken}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to create user.");
        }

        var usersResponse = await newClient.GetUsers(_realmName, $"Bearer {adminToken}");
        var userId = usersResponse.First(u => u.Email == _adminEmail).Id;

        ThrowHelper.ThrowIfNull(userId);

        var groupsResponse = await newClient.GetGroups(_realmName, "Admin", $"Bearer {adminToken}");
        var groupAdminId = groupsResponse.First().Id;

        ThrowHelper.ThrowIfNull(groupAdminId);

        var addUserToGroupResponse = await newClient.AddUserToGroup(_realmName, groupAdminId, userId, $"Bearer {adminToken}");

        if (!addUserToGroupResponse.IsSuccessStatusCode)
        {
            throw new Exception("Failed to add user to group.");
        }
    }

    public async Task<string> GetClientSecret()
    {
        var adminToken = await GetAdminToken();
        var baseRealm = _keycloakContainer.GetBaseAddress();
        var newClient = RestService.For<IKeycloakClientApi>(baseRealm, GetRefitSettings());

        var clients = await newClient.GetClients(_realmName, $"Bearer {adminToken}");
        var client = clients.FirstOrDefault(c => c.ClientId == _clientId);

        if (client is null)
        {
            throw new Exception("Client not found.");
        }

        await newClient.GenerateClientSecret(_realmName, client.Id, $"Bearer {adminToken}");

        var clientSecretResponse = await newClient.GetClients(_realmName, $"Bearer {adminToken}");
        var newClientSecret = clientSecretResponse.First(c => c.ClientId == _clientId).Secret;

        return newClientSecret;
    }
}