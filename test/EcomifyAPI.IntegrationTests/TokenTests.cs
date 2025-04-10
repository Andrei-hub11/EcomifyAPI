using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.IntegrationTests.Builders;
using EcomifyAPI.IntegrationTests.Converters;
using EcomifyAPI.IntegrationTests.Fixture;

using Shouldly;

using Xunit.Abstractions;

namespace EcomifyAPI.IntegrationTests;

[Collection("Database")]
public class TokenTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";
    private readonly ITestOutputHelper _output;
    private string _userId = string.Empty;
    private string _accessToken = string.Empty;
    private string _refreshToken = string.Empty;

    public TokenTests(AppHostFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _client = fixture.CreateClient();
    }

    private async Task AuthenticateUser()
    {
        var uniqueId = Guid.NewGuid().ToString();
        var registerRequest = new RegisterRequestBuilder()
            .WithUserName($"tokentest-{uniqueId}")
            .WithEmail($"token-{uniqueId}@test.com")
            .WithPassword("Token123!@#")
            .Build();

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/register", registerRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        _userId = result!.User.Id;
        _accessToken = result.AccessToken;
        _refreshToken = result.RefreshToken;
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewAccessToken()
    {
        // Arrange
        await AuthenticateUser();

        var request = new UpdateAccessTokenRequestDTO(_refreshToken);

        // Remove access token to simulate expired token
        _fixture.ClearAccessToken(new Uri(_baseUrl));

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/token-renew", request);
        var result = await response.Content.ReadFromJsonAsync<UpdateAccessTokenResponseDTO>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result.AccessToken.ShouldNotBeNullOrEmpty();
        result.AccessToken.ShouldNotBe(_accessToken); // New token should be different
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ShouldReturnValidationError()
    {
        // Arrange
        await AuthenticateUser();
        var request = new UpdateAccessTokenRequestDTO(string.Empty);

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/token-renew", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    /*     [Fact]
        public async Task RefreshToken_WithExpiredToken_ShouldReturnUnauthorized()
        {
            // Arrange
            await AuthenticateUser();

            _fixture.ClearCookies();

            var request = new UpdateAccessTokenRequestDTO("");

            // Act
            var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/token-renew", request);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        } */

    [Fact]
    public async Task TokenMiddleware_ShouldReturnUnauthorized_WhenBothTokensAreMissing()
    {
        // Arrange
        await AuthenticateUser();

        // Clear all cookies to simulate no tokens
        _fixture.ClearCookies();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/account/profile");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ResetAsync();
}