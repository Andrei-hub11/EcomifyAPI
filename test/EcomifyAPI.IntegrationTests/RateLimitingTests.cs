using System.Net;
using System.Net.Http.Json;

using EcomifyAPI.IntegrationTests.Fixture;

using Microsoft.AspNetCore.Mvc;

using Shouldly;

namespace EcomifyAPI.IntegrationTests;

[Collection("Database")]
public class RateLimitingTests : IAsyncLifetime
{
    private readonly HttpClient _client = default!;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";

    public RateLimitingTests(AppHostFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnTooManyRequestsAfterExceedingLimit()
    {
        var testId = Guid.NewGuid().ToString();
        _client.DefaultRequestHeaders.Add("X-Test-ID", testId);

        var tasks = Enumerable.Range(0, 10)
        .Select(_ => _client.GetAsync($"{_baseUrl}/products"))
        .ToList();

        var responses = await Task.WhenAll(tasks);

        var successResponse = responses.FirstOrDefault(response => response.StatusCode == HttpStatusCode.OK);
        successResponse.ShouldNotBeNull();

        var errorResponse = responses.FirstOrDefault(response => response.StatusCode == HttpStatusCode.TooManyRequests);
        errorResponse.ShouldNotBeNull();

        var problemDetails = await errorResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Detail.ShouldBe("You have made too many requests. Please try again later.");
        problemDetails.Title.ShouldBe("Too many requests");
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _client.DefaultRequestHeaders.Remove("X-Test-ID");
        return Task.CompletedTask;
    }
}