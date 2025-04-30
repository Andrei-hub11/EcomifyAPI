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
public class OrderTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";
    private readonly ITestOutputHelper _output;
    private string _adminId = string.Empty;
    private string _userId = string.Empty;
    private Guid _productId = Guid.Empty;
    private Guid _categoryId = Guid.Empty;

    public OrderTests(AppHostFixture fixture, ITestOutputHelper output)
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

    private async Task AuthenticateUser()
    {
        var uniqueId = Guid.NewGuid().ToString();
        var registerRequest = new RegisterRequestBuilder()
            .WithUserName($"user-{uniqueId}")
            .WithEmail($"user-{uniqueId}@test.com")
            .WithPassword("User123!@#")
            .Build();

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/account/register", registerRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        _userId = result!.User.Id;
    }

    private async Task SetupProductAndCart()
    {
        await AuthenticateAdmin();
        // Create category
        var categoryRequest = new CategoryRequestBuilder()
            .WithName($"Category-{Guid.NewGuid()}")
            .WithDescription("Test category")
            .Build();

        var categoryResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        categoryResponse.EnsureSuccessStatusCode();

        var getCategoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await getCategoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<CategoryResponseDTO>() },
            });

        _categoryId = categories![0].Id;

        // Create product
        var productRequest = new ProductRequestBuilder()
            .WithName($"Product-{Guid.NewGuid()}")
            .WithDescription("Test product")
            .WithPrice(199.99m)
            .WithStock(10)
            .WithCategories(_categoryId)
            .Build();

        var productResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        productResponse.EnsureSuccessStatusCode();

        var getProductsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var products = await getProductsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<ProductResponseDTO>() },
            });

        _productId = products![0].Id;

        // Add product to cart
        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(_productId)
            .WithQuantity(2)
            .Build();

        await AuthenticateUser();

        // Ensure cart exists
        await _client.GetAsync($"{_baseUrl}/carts/{_userId}");

        // Add item to cart
        var addItemResponse = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_userId}", addItemRequest);
        addItemResponse.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateOrder()
    {
        var creditCardDetails = new CreditCardDetailsBuilder()
            .WithCardNumber("4111111111111111")
            .WithCardholderName("Test User")
            .WithExpiryDate("12/25")
            .WithCvv("123")
            .Build();

        var shippingAddress = new AddressRequestBuilder()
            .WithStreet("Shipping Street")
            .WithNumber(123)
            .WithCity("São Paulo")
            .WithState("SP")
            .WithZipCode("01234-567")
            .WithCountry("Brazil")
            .WithComplement("Shipping Complement")
            .Build();

        var billingAddress = new AddressRequestBuilder()
            .WithStreet("Billing Street")
            .WithNumber(456)
            .WithCity("São Paulo")
            .WithState("SP")
            .WithZipCode("01234-567")
            .WithCountry("Brazil")
            .WithComplement("Billing Complement")
            .Build();

        var paymentRequest = new PaymentRequestBuilder()
            .WithUserId(_userId)
            .WithPaymentMethod(PaymentMethodEnumDTO.CreditCard)
            .WithCreditCardDetails(creditCardDetails)
            .WithShippingAddress(shippingAddress)
            .WithBillingAddress(billingAddress)
            .Build();

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/payments/process", paymentRequest);

        response.EnsureSuccessStatusCode();

        await response.Content.ReadFromJsonAsync<PaymentResponseDTO>();

        // Get orders for user to find the created order
        var ordersResponse = await _client.GetAsync($"{_baseUrl}/orders/{_userId}/user");
        ordersResponse.EnsureSuccessStatusCode();

        var orders = await ordersResponse.Content.ReadFromJsonAsync<IReadOnlyList<OrderResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<OrderResponseDTO>() },
            });

        return orders![0].Id;
    }

    [Fact]
    public async Task GetOrders_ForUser_ShouldReturnUserOrders()
    {
        // Arrange
        await SetupProductAndCart();
        await CreateOrder();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/orders/{_userId}/user");
        var orders = await response.Content.ReadFromJsonAsync<IReadOnlyList<OrderResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<OrderResponseDTO>() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        orders.ShouldNotBeNull();
        orders.ShouldNotBeEmpty();
        orders[0].UserId.ShouldBe(_userId);
        orders[0].Status.ShouldBe(OrderStatusDTO.Confirmed);
        orders[0].Items.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetFilteredOrders_AsAdmin_ShouldReturnFilteredOrders()
    {
        // Arrange
        await SetupProductAndCart();
        await CreateOrder();
        await AuthenticateAdmin();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/orders/filter?page=1&pageSize=10");
        var paginatedOrders = await response.Content.ReadFromJsonAsync<PaginatedResponseDTO<OrderResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        paginatedOrders.ShouldNotBeNull();
        paginatedOrders.Items.ShouldNotBeEmpty();
        paginatedOrders.PageNumber.ShouldBe(1);
        paginatedOrders.PageSize.ShouldBe(10);
        paginatedOrders.TotalCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetFilteredOrders_WithoutAdminRole_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateUser();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/orders/filter?page=1&pageSize=10");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOrder_WithValidId_ShouldReturnOrder()
    {
        // Arrange
        await SetupProductAndCart();
        var orderId = await CreateOrder();
        await AuthenticateAdmin();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/orders/{orderId}");
        var order = await response.Content.ReadFromJsonAsync<OrderResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        order.ShouldNotBeNull();
        order.Id.ShouldBe(orderId);
        order.UserId.ShouldBe(_userId);
        order.Items.ShouldNotBeEmpty();
        order.Items.Count.ShouldBe(1);
        order.Items[0].Quantity.ShouldBe(2);
    }

    [Fact]
    public async Task GetOrder_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/orders/{invalidId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAsCompleted_WithValidOrder_ShouldUpdateStatus()
    {
        // Arrange
        await SetupProductAndCart();
        var orderId = await CreateOrder();
        await AuthenticateAdmin();

        // First mark as shipped (required before completing)
        var shipResponse = await _client.PutAsync($"{_baseUrl}/orders/{orderId}/shipped", null);
        shipResponse.EnsureSuccessStatusCode();

        // Act - Mark as completed
        var response = await _client.PutAsync($"{_baseUrl}/orders/{orderId}/completed", null);
        var isUpdated = await response.Content.ReadFromJsonAsync<bool>();

        // Get the order to verify
        var orderResponse = await _client.GetAsync($"{_baseUrl}/orders/{orderId}");
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        isUpdated.ShouldBeTrue();
        order.ShouldNotBeNull();
        order.Status.ShouldBe(OrderStatusDTO.Completed);
    }

    [Fact]
    public async Task MarkAsShipped_WithValidOrder_ShouldUpdateStatus()
    {
        // Arrange
        await SetupProductAndCart();
        var orderId = await CreateOrder();
        await AuthenticateAdmin();

        // Act
        var response = await _client.PutAsync($"{_baseUrl}/orders/{orderId}/shipped", null);
        var isUpdated = await response.Content.ReadFromJsonAsync<bool>();

        // Get the order to verify
        var orderResponse = await _client.GetAsync($"{_baseUrl}/orders/{orderId}");
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        isUpdated.ShouldBeTrue();
        order.ShouldNotBeNull();
        order.Status.ShouldBe(OrderStatusDTO.Shipped);
    }

    [Fact]
    public async Task OrderStatusUpdates_WithoutAdminRole_ShouldReturnForbidden()
    {
        // Arrange
        await SetupProductAndCart();
        var orderId = await CreateOrder();

        await AuthenticateUser();

        // Act - Try to mark as shipped
        var shippedResponse = await _client.PutAsync($"{_baseUrl}/orders/{orderId}/shipped", null);

        // Act - Try to mark as completed
        var completedResponse = await _client.PutAsync($"{_baseUrl}/orders/{orderId}/completed", null);

        // Assert
        shippedResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        completedResponse.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    /*     [Fact]
        public async Task DeleteOrder_AsAdmin_ShouldRemoveOrder()
        {
            // Arrange
            await SetupProductAndCart();
            var orderId = await CreateOrder();

            await AuthenticateAdmin();

            // Act
            var response = await _client.DeleteAsync($"{_baseUrl}/orders/{orderId}");
            var isDeleted = await response.Content.ReadFromJsonAsync<bool>();

            // Try to get the deleted order
            var getResponse = await _client.GetAsync($"{_baseUrl}/orders/{orderId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            isDeleted.ShouldBeTrue();
            getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteOrder_WithoutAdminRole_ShouldReturnForbidden()
        {
            // Arrange
            await SetupProductAndCart();
            var orderId = await CreateOrder();

            // Act
            var response = await _client.DeleteAsync($"{_baseUrl}/orders/{orderId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        } */

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ResetAsync();
}