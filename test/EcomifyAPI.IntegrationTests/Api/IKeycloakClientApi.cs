using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.IntegrationTests.Contracts.Requests;
using EcomifyAPI.IntegrationTests.Contracts.Response;

using Refit;

namespace EcomifyAPI.IntegrationTests.Api;

public interface IKeycloakClientApi
{
    [Get("/admin/realms/{realm}/users")]
    Task<IEnumerable<UserMapping>> GetUsers([AliasAs("realm")] string realm, [Header("Authorization")] string authorization);
    [Get("/admin/realms/{realm}/groups?search={groupName}")]
    Task<IEnumerable<GroupResponseDTO>> GetGroups([AliasAs("realm")] string realm,
    [AliasAs("groupName")] string groupName, [Header("Authorization")] string authorization);
    [Get("/admin/realms/{realm}/clients")]
    Task<IEnumerable<KeycloakClient>> GetClients([AliasAs("realm")] string realm,
 [Header("Authorization")] string authorization);
    [Post("/realms/master/protocol/openid-connect/token")]
    Task<KeycloakToken> GetToken([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> payload);
    [Post("/admin/realms/{realm}/users")]
    Task<HttpResponseMessage> CreateUser([Body(BodySerializationMethod.Serialized)] KeycloakUser user,
    [AliasAs("realm")] string realm, [Header("Authorization")] string authorization);
    [Post("/admin/realms/{realm}/clients/{clientId}/client-secret")]
    Task<KeycloakClient> GenerateClientSecret([AliasAs("realm")] string realm, [AliasAs("clientId")] string clientId,
    [Header("Authorization")] string authorization);
    [Put("/admin/realms/{realm}/users/{userId}/groups/{groupId}")]
    Task<HttpResponseMessage> AddUserToGroup([AliasAs("realm")] string realm, [AliasAs("groupId")] string groupId,
    [AliasAs("userId")] string userId, [Header("Authorization")] string authorization);
}