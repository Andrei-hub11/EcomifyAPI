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
public class CartTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";
    private readonly ITestOutputHelper _output;
    private string _adminId = string.Empty;
    private string _accessToken = string.Empty;

    public CartTests(AppHostFixture fixture, ITestOutputHelper output)
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
        _accessToken = result.AccessToken;
    }

    [Fact]
    public async Task GetCart_WhenAuthenticated_ShouldReturnEmptyCart()
    {
        // Arrange
        await AuthenticateAdmin();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");
        var cart = await response.Content.ReadFromJsonAsync<CartResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        cart.ShouldNotBeNull();
        cart.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCart_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var nonExistentUserId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/carts/{nonExistentUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItem_WhenAuthenticated_ShouldAddItemToCart()
    {
        // Arrange
        await AuthenticateAdmin();

        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Technology")
            .WithDescription("Tech products")
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
        var categoryId = categories[0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Mechanical Keyboard")
            .WithPrice(299.90m)
            .WithCategories(categoryId)
            .Build();

        var productCreateResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        productCreateResponse.EnsureSuccessStatusCode();

        var getProductsResponse = await _client.GetAsync($"{_baseUrl}/products");
        getProductsResponse.EnsureSuccessStatusCode();

        var products = await getProductsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<ProductResponseDTO>() },
        });

        products.ShouldNotBeNull();
        products.ShouldNotBeEmpty();
        var productId = products[0].Id;

        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(2)
            .Build();

        // force the creation of the cart
        _ = await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addItemRequest);
        response.EnsureSuccessStatusCode();

        var cart = await response.Content.ReadFromJsonAsync<CartResponseDTO>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<CartItemResponseDTO>() },
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        cart.ShouldNotBeNull();
        cart.Items.Count.ShouldBe(1);
        cart.Items[0].Quantity.ShouldBe(2);
        cart.Items[0].ProductId.ShouldBe(productId);
    }

    [Fact]
    public async Task AddItem_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var nonExistentProductId = Guid.NewGuid();

        var request = new AddItemRequestBuilder()
            .WithProductId(nonExistentProductId)
            .WithQuantity(1)
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddItem_WithInvalidQuantity_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create a product first
        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Test Category")
            .WithDescription("Test Description")
            .Build();

        var categoryResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        var categories = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categoryId = (await categories.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>())![0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Test Product")
            .WithPrice(100m)
            .WithCategories(categoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        var products = await _client.GetAsync($"{_baseUrl}/products");
        var productId = (await products.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>())![0].Id;

        var request = new AddItemRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(-1) // Invalid quantity
            .Build();

        // force the creation of the cart
        _ = await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateItemQuantity_WhenAuthenticated_ShouldUpdateQuantity()
    {
        // Arrange
        await AuthenticateAdmin();

        var categoryRequest = new CategoryRequestBuilder()
           .WithName("Technology")
           .WithDescription("Tech products")
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
        var categoryId = categories[0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Mouse Gamer")
            .WithPrice(150.00m)
            .WithCategories(categoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        var productsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var product = (await productsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>())![0];

        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(product.Id)
            .WithQuantity(1)
            .Build();

        // force the creation of the cart
        _ = await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addItemRequest);

        // Act: Update the quantity
        var updateQuantityRequest = new UpdateItemQuantityRequestBuilder()
            .WithProductId(product.Id)
            .WithQuantity(5)
            .Build();

        var response = await _client.PutAsJsonAsync($"{_baseUrl}/carts/{_adminId}/items", updateQuantityRequest);
        response.EnsureSuccessStatusCode();

        var cart = await response.Content.ReadFromJsonAsync<CartResponseDTO>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<CartItemResponseDTO>() },
        });

        // Assert
        cart.ShouldNotBeNull();
        cart!.Items.ShouldContain(i => i.ProductId == product.Id && i.Quantity == 5);
    }

    [Fact]
    public async Task UpdateItemQuantity_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var nonExistentProductId = Guid.NewGuid();

        var request = new UpdateItemQuantityRequestBuilder()
            .WithProductId(nonExistentProductId)
            .WithQuantity(1)
            .Build();

        // Act
        var response = await _client.PutAsJsonAsync($"{_baseUrl}/carts/{_adminId}/items", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateItemQuantity_WithInvalidQuantity_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create and add a product to cart first
        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Test Category")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        var categories = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categoryId = (await categories.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>())![0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Test Product")
            .WithPrice(100m)
            .WithCategories(categoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        var products = await _client.GetAsync($"{_baseUrl}/products");
        var productId = (await products.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>())![0].Id;

        // Add item to cart
        var addRequest = new AddItemRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(1)
            .Build();

        // force the creation of the cart
        _ = await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addRequest);

        // Try to update with invalid quantity
        var updateRequest = new UpdateItemQuantityRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(0) // Invalid quantity
            .Build();

        // Act
        var response = await _client.PutAsJsonAsync($"{_baseUrl}/carts/{_adminId}/items", updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RemoveItem_WhenAuthenticated_ShouldRemoveItemFromCart()
    {
        // Arrange
        await AuthenticateAdmin();

        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Technology")
            .WithDescription("Tech products")
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
        var categoryId = categories[0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Wireless Mouse")
            .WithPrice(150.00m)
            .WithCategories(categoryId)
            .Build();

        var productResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        productResponse.EnsureSuccessStatusCode();

        // Get products
        var getProducts = await _client.GetAsync($"{_baseUrl}/products");
        getProducts.EnsureSuccessStatusCode();

        var products = await getProducts.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<ProductResponseDTO>() }
            });

        products.ShouldNotBeNull();
        products.Count.ShouldBeGreaterThan(0);

        // force the creation of the cart
        _ = await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(products[0].Id)
            .WithQuantity(1)
            .Build();

        var addItemResponse = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addItemRequest);
        addItemResponse.EnsureSuccessStatusCode();

        var addedCart = await addItemResponse.Content.ReadFromJsonAsync<CartResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() }
            });

        addedCart.ShouldNotBeNull();
        addedCart.Items.Count.ShouldBe(1);

        // Act: Remove the item
        var removeResponse = await _client.DeleteAsync($"{_baseUrl}/carts/{_adminId}/{products[0].Id}");
        removeResponse.EnsureSuccessStatusCode();

        var updatedCart = await removeResponse.Content.ReadFromJsonAsync<CartResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() }
            });

        // Assert
        updatedCart.ShouldNotBeNull();
        updatedCart.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task RemoveItem_WithNonExistentProduct_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"{_baseUrl}/carts/{_adminId}/{nonExistentProductId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ClearCart_WhenAuthenticated_ShouldRemoveAllItems()
    {
        // Arrange
        await AuthenticateAdmin();

        var categoryRequest = new CategoryRequestBuilder().WithName("Tech").Build();
        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<CategoryResponseDTO>() }
        });
        var categoryId = categories![0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Headset")
            .WithPrice(200.00m)
            .WithCategories(categoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        var productsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var product = (await productsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<ProductResponseDTO>() }
        }))![0];

        // force the creation of the cart
        _ = await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(product.Id)
            .WithQuantity(3)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addItemRequest);

        // Act: Clear the cart
        var response = await _client.DeleteAsync($"{_baseUrl}/carts/{_adminId}");
        response.EnsureSuccessStatusCode();

        var cart = await response.Content.ReadFromJsonAsync<CartResponseDTO>();

        // Assert
        cart.ShouldNotBeNull();
        cart!.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task ClearCart_WhenCartNotFound_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var nonExistentUserId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.DeleteAsync($"{_baseUrl}/carts/{nonExistentUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Endpoints_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid();

        _fixture.ClearCookies();

        // Act & Assert
        // Test GET cart
        var getResponse = await _client.GetAsync($"{_baseUrl}/carts/{userId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Test POST add item
        var addRequest = new AddItemRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(1)
            .Build();
        var addResponse = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{userId}", addRequest);
        addResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Test PUT update quantity
        var updateRequest = new UpdateItemQuantityRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(2)
            .Build();
        var updateResponse = await _client.PutAsJsonAsync($"{_baseUrl}/carts/{userId}/items", updateRequest);
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Test DELETE remove item
        var removeResponse = await _client.DeleteAsync($"{_baseUrl}/carts/{userId}/{productId}");
        removeResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        // Test DELETE clear cart
        var clearResponse = await _client.DeleteAsync($"{_baseUrl}/carts/{userId}");
        clearResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }


    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ResetAsync();
}