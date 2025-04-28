using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.IntegrationTests.Builders;
using EcomifyAPI.IntegrationTests.Converters;
using EcomifyAPI.IntegrationTests.Fixture;

using Shouldly;

using Xunit.Abstractions;

namespace EcomifyAPI.IntegrationTests;

[Collection("Database")]
public class DiscountTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";
    private readonly ITestOutputHelper _output;
    private string _adminId = string.Empty;
    private Guid _categoryId = Guid.Empty;

    public DiscountTests(AppHostFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _client = fixture.CreateClient();
    }

    private async Task AuthenticateAdmin()
    {
        var uniqueId = Guid.NewGuid().ToString();
        var registerRequest = new RegisterRequestBuilder()
            .WithUserName($"admin-{uniqueId}")
            .WithEmail($"admin-{uniqueId}@test.com")
            .WithPassword("Admin123!@#")
            .Build();

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/test-utils/create-admin", registerRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        _adminId = result!.User.Id;
    }

    private async Task<Guid> CreateCategory()
    {
        var categoryRequest = new CategoryRequestBuilder()
            .WithName($"Discount Category {Guid.NewGuid()}")
            .WithDescription("Test category for discounts")
            .Build();

        var categoryCreateResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        categoryCreateResponse.EnsureSuccessStatusCode();

        var getCategoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        getCategoriesResponse.EnsureSuccessStatusCode();

        var categories = await getCategoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<CategoryResponseDTO>() },
        });

        categories.ShouldNotBeNull();
        categories.ShouldNotBeEmpty();
        return categories[0].Id;
    }

    [Fact]
    public async Task GetDiscounts_WhenAdmin_ShouldReturnDiscounts()
    {
        // Arrange
        await AuthenticateAdmin();
        var filter = new DiscountFilterRequestBuilder()
            .WithPageSize(10)
            .WithPageNumber(1)
            .Build();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/discounts?PageSize={filter.PageSize}&PageNumber={filter.PageNumber}");
        var discounts = await response.Content.ReadFromJsonAsync<PaginatedResponseDTO<DiscountResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        discounts.ShouldNotBeNull();
        discounts.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetDiscountById_WithValidId_ShouldReturnDiscount()
    {
        // Arrange
        await AuthenticateAdmin();
        _categoryId = await CreateCategory();

        var createRequest = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Fixed)
            .WithFixedAmount(10.00m)
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddDays(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .WithCategories(_categoryId)
            .Build();

        var createResponse = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDiscount = await createResponse.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/discounts/{createdDiscount!.Id}");
        var discount = await response.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        discount.ShouldNotBeNull();
        discount.Id.ShouldBe(createdDiscount.Id);
    }

    [Fact]
    public async Task GetDiscountById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/discounts/{invalidId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateDiscount_WithValidFixedDiscountData_ShouldCreateDiscount()
    {
        // Arrange
        await AuthenticateAdmin();
        _categoryId = await CreateCategory();

        var request = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Fixed)
            .WithFixedAmount(10.00m)
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddDays(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .WithAutoApply(true)
            .WithCategories(_categoryId)
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", request);
        var result = await response.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result.FixedAmount.ShouldBe(10.00m);
        result.Percentage.ShouldBeNull();
        result.DiscountType.ShouldBe(DiscountTypeEnum.Fixed);
        result.AutoApply.ShouldBeTrue();
        result.Categories.ShouldContain(c => c.Id == _categoryId);
    }

    [Fact]
    public async Task CreateDiscount_WithValidPercentageDiscountData_ShouldCreateDiscount()
    {
        // Arrange
        await AuthenticateAdmin();
        _categoryId = await CreateCategory();

        var request = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Percentage)
            .WithPercentage(15.00m)
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddDays(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .WithAutoApply(true)
            .WithCategories(_categoryId)
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", request);
        var result = await response.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result.FixedAmount.ShouldBeNull();
        result.Percentage.ShouldBe(15.00m);
        result.DiscountType.ShouldBe(DiscountTypeEnum.Percentage);
        result.AutoApply.ShouldBeTrue();
        result.Categories.ShouldContain(c => c.Id == _categoryId);
    }

    [Fact]
    public async Task CreateDiscount_WithValidCouponDiscountData_ShouldCreateDiscount()
    {
        // Arrange
        await AuthenticateAdmin();
        _categoryId = await CreateCategory();
        var couponCode = $"TEST{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var request = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Coupon)
            .WithCouponCode(couponCode)
            .WithFixedAmount(20.00m)
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddDays(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", request);
        var result = await response.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result.Code.ShouldNotBeNull();
        result.Code.ShouldBe(couponCode);
        result.FixedAmount.ShouldBe(20.00m);
        result.DiscountType.ShouldBe(DiscountTypeEnum.Coupon);
    }

    [Fact]
    public async Task CreateDiscount_WithInvalidData_ShouldReturnValidationErrors()
    {
        // Arrange
        await AuthenticateAdmin();

        // Missing required fields for Fixed discount type
        var request = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Fixed)
            .WithPercentage(15.00m) // Wrong - should be FixedAmount
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddDays(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateDiscount_WithCouponTypeButNoCode_ShouldReturnValidationErrors()
    {
        // Arrange
        await AuthenticateAdmin();

        // Missing coupon code for Coupon discount type
        var request = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Coupon)
            .WithFixedAmount(20.00m)
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddDays(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task DeactivateDiscount_WithValidId_ShouldDeactivateDiscount()
    {
        // Arrange
        await AuthenticateAdmin();
        _categoryId = await CreateCategory();

        var createRequest = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Fixed)
            .WithFixedAmount(10.00m)
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddDays(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .WithCategories(_categoryId)
            .Build();

        var createResponse = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDiscount = await createResponse.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Act
        var response = await _client.PutAsync($"{_baseUrl}/discounts/{createdDiscount!.Id}/deactivate", null);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldBeTrue();

        // Verify the discount is deactivated
        var getResponse = await _client.GetAsync($"{_baseUrl}/discounts/{createdDiscount.Id}");
        var updatedDiscount = await getResponse.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        updatedDiscount.ShouldNotBeNull();
        updatedDiscount.AutoApply.ShouldBe(createdDiscount.AutoApply);
    }

    [Fact]
    public async Task DeactivateDiscount_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"{_baseUrl}/discounts/{invalidId}/deactivate", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDiscount_WithValidId_ShouldDeleteDiscount()
    {
        // Arrange
        await AuthenticateAdmin();
        _categoryId = await CreateCategory();

        var createRequest = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Fixed)
            .WithFixedAmount(10.00m)
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddDays(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .WithCategories(_categoryId)
            .Build();

        var createResponse = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDiscount = await createResponse.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Act
        var response = await _client.DeleteAsync($"{_baseUrl}/discounts/{createdDiscount!.Id}");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldBeTrue();

        // Verify the discount is deleted
        var getResponse = await _client.GetAsync($"{_baseUrl}/discounts/{createdDiscount.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDiscount_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"{_baseUrl}/discounts/{invalidId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ResetAsync();
}