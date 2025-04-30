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
public class CartTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";
    private readonly ITestOutputHelper _output;
    private string _adminId = string.Empty;

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
        var product = products[0];

        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(product.Id)
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
        cart.Items[0].ProductId.ShouldBe(product.Id);
        cart.Items[0].Product.Name.ShouldBe(product.Name);
        cart.Items[0].Product.Price.ShouldBe(product.Price);
        cart.Items[0].Product.ImageUrl.ShouldBe(product.ImageUrl);
    }

    [Fact]
    public async Task AddItem_WhenItemAlreadyExists_ShouldIncreaseQuantityInsteadOfAddingNewItem()
    {
        // Arrange
        await AuthenticateAdmin();

        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Gadgets")
            .WithDescription("Cool gadgets")
            .Build();

        var categoryCreateResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        categoryCreateResponse.EnsureSuccessStatusCode();

        var getCategoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await getCategoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>();
        var categoryId = categories![0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Bluetooth Speaker")
            .WithPrice(199.90m)
            .WithCategories(categoryId)
            .Build();

        var productCreateResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        productCreateResponse.EnsureSuccessStatusCode();

        var getProductsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var products = await getProductsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>();
        var product = products![0];

        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(product.Id)
            .WithQuantity(1)
            .Build();

        // Force cart creation
        _ = await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        // Act - Add the product once
        var firstAddResponse = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addItemRequest);
        firstAddResponse.EnsureSuccessStatusCode();

        // Act - Add the same product again
        var secondAddResponse = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addItemRequest);
        secondAddResponse.EnsureSuccessStatusCode();

        // Assert - Should still have only one item in cart, but quantity = 2
        var cartResponse = await secondAddResponse.Content.ReadFromJsonAsync<CartResponseDTO>();
        cartResponse.ShouldNotBeNull();
        cartResponse.Items.Count.ShouldBe(1);
        cartResponse.Items[0].ProductId.ShouldBe(product.Id);
        cartResponse.Items[0].Quantity.ShouldBe(2); // 1 + 1 = 2
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
    public async Task ApplyDiscount_WithValidDiscount_ShouldApplyDiscountToCart()
    {
        // Arrange
        await AuthenticateAdmin();

        // 1. Create a category
        var categoryRequest = new CategoryRequestBuilder()
            .WithName($"Category-{Guid.NewGuid()}")
            .WithDescription("Test category for discount")
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

        var categoryId = categories![0].Id;

        // 2. Create a product
        var productRequest = new ProductRequestBuilder()
            .WithName($"Product-{Guid.NewGuid()}")
            .WithDescription("Test product for discount")
            .WithPrice(199.99m)
            .WithStock(10)
            .WithCategories(categoryId)
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

        var productId = products![0].Id;

        // 3. Create a discount
        var createDiscountRequest = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Fixed)
            .WithFixedAmount(20.00m)
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddSeconds(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .WithAutoApply(false)
            .WithCategories(categoryId)
            .Build();

        var createDiscountResponse = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", createDiscountRequest);
        createDiscountResponse.EnsureSuccessStatusCode();

        var discountResponseDTO = await createDiscountResponse.Content.ReadFromJsonAsync<DiscountResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // 4. Add product to cart
        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(2)
            .Build();

        // Force cart creation
        await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        var addItemResponse = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addItemRequest);
        addItemResponse.EnsureSuccessStatusCode();

        var cartBeforeDiscount = await addItemResponse.Content.ReadFromJsonAsync<CartResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Act: Apply the discount - only send the discount ID
        var applyDiscountRequest = new ApplyDiscountRequestBuilder()
            .WithDiscountId(discountResponseDTO!.Id)
            .Build();

        // Wait for making the discount available
        await Task.Delay(1000);

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}/discount", applyDiscountRequest);
        response.EnsureSuccessStatusCode();

        var cartAfterDiscount = await response.Content.ReadFromJsonAsync<CartResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        cartAfterDiscount.ShouldNotBeNull();
        cartAfterDiscount.Items.Count.ShouldBe(1);
        cartAfterDiscount.TotalWithDiscount.ShouldNotBeNull();
        cartAfterDiscount.TotalWithDiscount!.Amount.ShouldBeLessThan(cartBeforeDiscount!.TotalAmount.Amount);
        // Fixed amount of 20.00 discount should be applied
        cartAfterDiscount.TotalWithDiscount.Amount.ShouldBe(cartBeforeDiscount.TotalAmount.Amount - 20.00m);
    }

    [Fact]
    public async Task ApplyDiscount_WithInvalidDiscountId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();

        // Force cart creation
        await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        var invalidDiscountId = Guid.NewGuid();
        var applyDiscountRequest = new ApplyDiscountRequestBuilder()
            .WithDiscountId(invalidDiscountId)
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}/discount", applyDiscountRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApplyDiscount_WithPercentageDiscount_ShouldCorrectlyCalculateDiscount()
    {
        // Arrange
        await AuthenticateAdmin();

        // 1. Create a category
        var categoryRequest = new CategoryRequestBuilder()
            .WithName($"Category-{Guid.NewGuid()}")
            .WithDescription("Percentage discount category")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        var categories = await (await _client.GetAsync($"{_baseUrl}/products/categories")).Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>();
        var categoryId = categories![0].Id;

        // 2. Create a product with a known price for easy percentage calculation
        var productRequest = new ProductRequestBuilder()
            .WithName($"Product-{Guid.NewGuid()}")
            .WithPrice(100.00m) // Easy to calculate percentage
            .WithStock(10)
            .WithCategories(categoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        var products = await (await _client.GetAsync($"{_baseUrl}/products")).Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>();
        var productId = products![0].Id;

        // 3. Create a percentage discount (10%)
        var createDiscountRequest = new CreateDiscountRequestBuilder()
            .WithDiscountType(DiscountTypeEnum.Percentage)
            .WithPercentage(10) // 10% discount
            .WithMaxUses(100)
            .WithMinOrderAmount(50.00m)
            .WithValidFrom(DateTime.UtcNow.AddSeconds(1))
            .WithValidTo(DateTime.UtcNow.AddDays(30))
            .WithAutoApply(false)
            .WithCategories(categoryId)
            .Build();

        var createDiscountResponse = await _client.PostAsJsonAsync($"{_baseUrl}/discounts", createDiscountRequest);
        var discountResponseDTO = await createDiscountResponse.Content.ReadFromJsonAsync<DiscountResponseDTO>();

        // 4. Add product to cart (2 items at $100 each = $200 total)
        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(2)
            .Build();

        // Force cart creation
        await _client.GetAsync($"{_baseUrl}/carts/{_adminId}");

        var addItemResponse = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}", addItemRequest);
        var cartBeforeDiscount = await addItemResponse.Content.ReadFromJsonAsync<CartResponseDTO>();

        // Act: Apply the percentage discount
        var applyDiscountRequest = new ApplyDiscountRequestBuilder()
            .WithDiscountId(discountResponseDTO!.Id)
            .Build();

        // Wait for making the discount available
        await Task.Delay(1000);

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_adminId}/discount", applyDiscountRequest);
        var cartAfterDiscount = await response.Content.ReadFromJsonAsync<CartResponseDTO>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        cartAfterDiscount.ShouldNotBeNull();
        cartBeforeDiscount.ShouldNotBeNull();

        // Calculate expected discount: 10% of $200 = $20
        var expectedDiscount = cartBeforeDiscount!.TotalAmount.Amount * 0.10m;
        var expectedDiscountedTotal = cartBeforeDiscount.TotalAmount.Amount - expectedDiscount;

        cartAfterDiscount!.TotalWithDiscount.ShouldNotBeNull();
        cartAfterDiscount.TotalWithDiscount!.Amount.ShouldBe(expectedDiscountedTotal);
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