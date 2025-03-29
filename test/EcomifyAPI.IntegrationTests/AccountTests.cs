using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using EcomifyAPI.Contracts.Response;
using EcomifyAPI.IntegrationTests.Builders;
using EcomifyAPI.IntegrationTests.Converters;
using EcomifyAPI.IntegrationTests.Fixture;

using Shouldly;

using Xunit.Abstractions;

namespace EcomifyAPI.IntegrationTests;

[Collection("Database")]
public class AccountTests : IAsyncLifetime
{
    private readonly HttpClient _client = default!;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";
    private readonly ITestOutputHelper _output;

    public AccountTests(AppHostFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        var uniqueId = Guid.NewGuid().ToString();
        // Arrange
        var request = new RegisterRequestBuilder()
            .WithUserName($"testuser-{uniqueId}")
            .WithEmail($"test-{uniqueId}@example.com")
            .WithPassword("Test123!@#")
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            }
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result!.User.ShouldNotBeNull();
        result.User.Email.ShouldBe(request.Email);
        result.User.UserName.ShouldBe(request.UserName);
        result.AccessToken.ShouldNotBeNullOrEmpty();
        result.RefreshToken.ShouldNotBeNullOrEmpty();
        result.Roles.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString();
        // Arrange
        var request = new RegisterRequestBuilder()
            .WithUserName($"testuser-{uniqueId}")
            .WithEmail($"invalid-email-{uniqueId}")
            .WithPassword("Test123!@#")
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/register", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString();
        // Arrange
        var request = new RegisterRequestBuilder()
            .WithUserName($"testuser-{uniqueId}")
            .WithEmail($"duplicate-{uniqueId}@test.com")
            .WithPassword("Test123!@#")
            .Build();

        // First registration
        await _client.PostAsJsonAsync($"{_baseUrl}/account/register", request);

        // Second registration with same email
        var duplicateRequest = new RegisterRequestBuilder()
            .WithUserName($"testuser2-{uniqueId}")
            .WithEmail($"duplicate-{uniqueId}@test.com")
            .WithPassword("Test123!@#")
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"{_baseUrl}/account/register",
            duplicateRequest
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidPassword_ShouldReturnBadRequest()
    {
        var uniqueId = Guid.NewGuid().ToString();
        // Arrange
        var request = new RegisterRequestBuilder()
            .WithUserName($"testuser-{uniqueId}")
            .WithEmail($"test-{uniqueId}@example.com")
            .WithPassword("weak") // Too short and missing required characters
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/register", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        var uniqueId = Guid.NewGuid().ToString();
        // Arrange
        var registerRequest = new RegisterRequestBuilder()
            .WithUserName($"logintest-{uniqueId}")
            .WithEmail($"login-{uniqueId}@test.com")
            .WithPassword("Login123!@#")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/account/register", registerRequest);

        var loginRequest = new LoginRequestBuilder()
            .WithEmail($"login-{uniqueId}@test.com")
            .WithPassword("Login123!@#")
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            }
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result!.User.ShouldNotBeNull();
        result.User.Email.ShouldBe(loginRequest.Email);
        result.AccessToken.ShouldNotBeNullOrEmpty();
        result.RefreshToken.ShouldNotBeNullOrEmpty();
        result.Roles.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnNotFound()
    {
        // Arrange
        var loginRequest = new LoginRequestBuilder()
            .WithEmail("nonexistent@test.com")
            .WithPassword("WrongPass123!@#")
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var uniqueId = Guid.NewGuid().ToString();
        // Arrange
        var registerRequest = new RegisterRequestBuilder()
            .WithUserName($"invalidlogintest-{uniqueId}")
            .WithEmail($"invalidlogin-{uniqueId}@test.com")
            .WithPassword("Valid123!@#")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/account/register", registerRequest);

        var loginRequest = new LoginRequestBuilder()
            .WithEmail($"invalidlogin-{uniqueId}@test.com")
            .WithPassword("InvalidPassword123!@#")
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    public Task InitializeAsync()
       => Task.CompletedTask;

    public Task DisposeAsync()
        => _fixture.ResetAsync();
}