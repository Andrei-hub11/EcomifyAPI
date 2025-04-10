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
public class ProductTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";
    private readonly ITestOutputHelper _output;
    private string _adminId = string.Empty;

    public ProductTests(AppHostFixture fixture, ITestOutputHelper output)
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
    public async Task GetProducts_ShouldReturnCreatedProductWithCorrectData()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create categories
        var category1 = new CategoryRequestBuilder()
            .WithName("Computers 1")
            .WithDescription("Desktops, notebooks and computer accessories.")
            .Build();

        var category2 = new CategoryRequestBuilder()
            .WithName("Electronics 2")
            .WithDescription("Electronic products, like smartphones and notebooks.")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", category1);
        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", category2);

        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<CategoryResponseDTO>() },
        });
        var categoryIds = categories!.Where(c => c.Name == "Computers 1" || c.Name == "Electronics 2").Select(c => c.Id).ToArray();

        // Create product with categories
        var productRequest = new ProductRequestBuilder()
            .WithName("Smartphone X")
            .WithDescription("Smartphone with AMOLED 6.5 inch screen, 128GB storage.")
            .WithPrice(2499.99m)
            .WithCurrencyCode("BRL")
            .WithStock(50)
            .WithImageUrl("https://example.com/images/smartphone-x.jpg")
            .WithStatus(ProductStatusDTO.Active)
            .WithCategories(categoryIds)
            .Build();

        var createResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/products");
        var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<ProductResponseDTO>() },
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        products.ShouldNotBeNull();
        products!.Count.ShouldBe(1);

        var product = products[0];
        product.Name.ShouldBe(productRequest.Name);
        product.Description.ShouldBe(productRequest.Description);
        product.Price.ShouldBe(productRequest.Price);
        product.CurrencyCode.ShouldBe(productRequest.CurrencyCode);
        product.Stock.ShouldBe(productRequest.Stock);
        product.ImageUrl.ShouldBe(productRequest.ImageUrl);
        ((int)product.Status).ShouldBe((int)productRequest.Status);
        product.Categories.Count.ShouldBe(2);

        product.Categories.Any(c => c.Name == "Computers 1").ShouldBeTrue();
        product.Categories.Any(c => c.Name == "Electronics 2").ShouldBeTrue();
    }


    [Fact]
    public async Task GetProducts_ShouldReturnEmptyList_WhenNoProductsExist()
    {
        // Act
        await AuthenticateAdmin();

        var response = await _client.GetAsync($"{_baseUrl}/products");
        var products = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<ProductResponseDTO>() },
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        products.ShouldNotBeNull();
        products!.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();

        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/products/{invalidId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLowStockProducts_AsAdmin_ShouldReturnPaginatedLowStockProducts()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create category
        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Electronics")
            .WithDescription("Electronic products")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);

        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<List<CategoryResponseDTO>>();
        var categoryId = categories![0].Id;

        // Create products with different stock levels
        var product1 = new ProductRequestBuilder()
            .WithName("Low Stock Product")
            .WithDescription("Product with low stock")
            .WithPrice(99.99m)
            .WithStock(5) // Low stock
            .WithCategories(categoryId)
            .Build();

        var product2 = new ProductRequestBuilder()
            .WithName("Medium Stock Product")
            .WithDescription("Product with medium stock")
            .WithPrice(199.99m)
            .WithStock(15) // Medium stock
            .WithCategories(categoryId)
            .Build();

        var product3 = new ProductRequestBuilder()
            .WithName("High Stock Product")
            .WithDescription("Product with high stock")
            .WithPrice(299.99m)
            .WithStock(50) // High stock
            .WithCategories(categoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", product1);
        await _client.PostAsJsonAsync($"{_baseUrl}/products", product2);
        await _client.PostAsJsonAsync($"{_baseUrl}/products", product3);

        // Create filter to get products with stock <= 10
        var filter = new ProductFilterRequestBuilder()
            .WithStockThreshold(10)
            .WithPageSize(10)
            .WithPageNumber(1)
            .Build();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/products/low-stock?StockThreshold={filter.StockThreshold}&PageSize={filter.PageSize}&PageNumber={filter.PageNumber}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseDTO<ProductResponseDTO>>();
        result.ShouldNotBeNull();
        result!.Items.Count.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Low Stock Product");
        result.Items[0].Stock.ShouldBe(5);
        result.TotalCount.ShouldBe(1);
        result.PageSize.ShouldBe(10);
        result.PageNumber.ShouldBe(1);
        result.TotalPages.ShouldBe(1);
    }

    [Fact]
    public async Task GetLowStockProducts_WithNameFilter_ShouldReturnFilteredProducts()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create category
        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Electronics 33")
            .WithDescription("Electronic products")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);

        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<CategoryResponseDTO>() },
        });
        var categoryId = categories![0].Id;

        // Create products with different names and stock levels
        var product1 = new ProductRequestBuilder()
            .WithName("Smartphone X")
            .WithDescription("Product with low stock")
            .WithPrice(99.99m)
            .WithStock(5) // Low stock
            .WithCategories(categoryId)
            .Build();

        var product2 = new ProductRequestBuilder()
            .WithName("Tablet Y")
            .WithDescription("Product with low stock")
            .WithPrice(199.99m)
            .WithStock(8) // Low stock
            .WithCategories(categoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", product1);
        await _client.PostAsJsonAsync($"{_baseUrl}/products", product2);

        // Create filter to get products with stock <= 10 and name containing "Smartphone"
        var filter = new ProductFilterRequestBuilder()
            .WithStockThreshold(10)
            .WithPageSize(10)
            .WithPageNumber(1)
            .WithName("Smartphone")
            .Build();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/products/low-stock?StockThreshold={filter.StockThreshold}&PageSize={filter.PageSize}&PageNumber={filter.PageNumber}&Name={filter.Name}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseDTO<ProductResponseDTO>>();
        result.ShouldNotBeNull();
        result!.Items.Count.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Smartphone X");
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetLowStockProducts_WithCategoryFilter_ShouldReturnFilteredProducts()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create multiple categories
        var electronicsCategory = new CategoryRequestBuilder()
            .WithName("Electronics 200")
            .WithDescription("Electronic products")
            .Build();

        var clothingCategory = new CategoryRequestBuilder()
            .WithName("Clothing")
            .WithDescription("Clothing products")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", electronicsCategory);
        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", clothingCategory);

        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<CategoryResponseDTO>() },
        });

        var electronicsCategoryId = categories!.First(c => c.Name == "Electronics 200").Id;
        var clothingCategoryId = categories!.First(c => c.Name == "Clothing").Id;

        // Create products with different categories and low stock
        var product1 = new ProductRequestBuilder()
            .WithName("Smartphone")
            .WithDescription("Electronic product with low stock")
            .WithPrice(99.99m)
            .WithStock(5)
            .WithCategories(electronicsCategoryId)
            .Build();

        var product2 = new ProductRequestBuilder()
            .WithName("T-Shirt")
            .WithDescription("Clothing product with low stock")
            .WithPrice(19.99m)
            .WithStock(8)
            .WithCategories(clothingCategoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", product1);
        await _client.PostAsJsonAsync($"{_baseUrl}/products", product2);

        // Create filter to get products with stock <= 10 and category "Electronics"
        var filter = new ProductFilterRequestBuilder()
            .WithStockThreshold(10)
            .WithPageSize(10)
            .WithPageNumber(1)
            .WithCategory("Electronics 200")
            .Build();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/products/low-stock?StockThreshold={filter.StockThreshold}&PageSize={filter.PageSize}&PageNumber={filter.PageNumber}&Category={filter.Category}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseDTO<ProductResponseDTO>>();
        result.ShouldNotBeNull();
        result!.Items.Count.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Smartphone");
        result.Items[0].Categories.Any(c => c.Name == "Electronics 200").ShouldBeTrue();
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetLowStockProducts_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create category
        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Electronics")
            .WithDescription("Electronic products")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);

        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<CategoryResponseDTO>() },
        });
        var categoryId = categories![0].Id;

        // Create 15 products with low stock (to test pagination)
        for (int i = 1; i <= 15; i++)
        {
            var product = new ProductRequestBuilder()
                .WithName($"Product {i}")
                .WithDescription($"Low stock product {i}")
                .WithPrice(10.00m * i)
                .WithStock(5) // All have low stock
                .WithCategories(categoryId)
                .Build();

            await _client.PostAsJsonAsync($"{_baseUrl}/products", product);
        }

        // Create filter for page 2 with page size 5
        var filter = new ProductFilterRequestBuilder()
            .WithStockThreshold(10)
            .WithPageSize(5)
            .WithPageNumber(2)
            .Build();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/products/low-stock?StockThreshold={filter.StockThreshold}&PageSize={filter.PageSize}&PageNumber={filter.PageNumber}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseDTO<ProductResponseDTO>>();
        result.ShouldNotBeNull();
        result!.Items.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(15);
        result.PageSize.ShouldBe(5);
        result.PageNumber.ShouldBe(2);
        result.TotalPages.ShouldBe(3);

        // Should contain products 6-10 (second page)
        result.Items.Any(p => p.Name == "Product 6").ShouldBeTrue();
        result.Items.Any(p => p.Name == "Product 10").ShouldBeTrue();
    }

    [Fact]
    public async Task GetLowStockProducts_WithInvalidFilter_ShouldReturnValidationError()
    {
        // Arrange
        await AuthenticateAdmin();

        // Act - pass invalid parameters
        var response = await _client.GetAsync($"{_baseUrl}/products/low-stock?StockThreshold=-5&PageSize=0&PageNumber=-1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Stock threshold must be greater than 0");
        content.ShouldContain("Page size must be greater than 0");
        content.ShouldContain("Page number must be greater than 0");
    }

    [Fact]
    public async Task GetLowStockProducts_Unauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange - clear authentication
        _fixture.ClearCookies();

        var filter = new ProductFilterRequestBuilder().Build();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/products/low-stock?StockThreshold={filter.StockThreshold}&PageSize={filter.PageSize}&PageNumber={filter.PageNumber}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnEmptyList_WhenNoCategoriesExist()
    {
        // Act
        await AuthenticateAdmin();
        var response = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await response.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<CategoryResponseDTO>() },
        });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        categories.ShouldNotBeNull();
        categories!.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateCategory_AsAdmin_ShouldCreateCategory()
    {
        // Arrange
        await AuthenticateAdmin();
        var request = new CategoryRequestBuilder()
            .WithName("Electronics Category")
            .WithDescription("Electronic products")
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.ShouldBeTrue();

        // Verify category was created
        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<CategoryResponseDTO>() },
        });
        categories.ShouldNotBeNull();
        categories!.Count.ShouldBe(1);
        categories[0].Name.ShouldBe(request.Name);
        categories[0].Description.ShouldBe(request.Description);
    }

    [Fact]
    public async Task CreateCategory_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CategoryRequestBuilder()
            .WithName("Electronics Category")
            .WithDescription("Electronic products")
            .Build();

        // remove authorization
        _fixture.ClearCookies();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_AsAdmin_ShouldCreateProduct()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create category first
        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Electronics Category")
            .WithDescription("Electronic products")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<CategoryResponseDTO>() },
        });
        var categoryId = categories![0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Smartphone")
            .WithDescription("Latest smartphone")
            .WithPrice(999.99m)
            .WithStock(100)
            .WithCategories(categoryId)
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.ShouldBeTrue();

        // Verify product was created
        var productsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<ProductResponseDTO>() },
        });
        products.ShouldNotBeNull();
        products!.Count.ShouldBe(1);
        products[0].Name.ShouldBe(productRequest.Name);
        products[0].Description.ShouldBe(productRequest.Description);
        products[0].Price.ShouldBe(productRequest.Price);
        products[0].Stock.ShouldBe(productRequest.Stock);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new ProductRequestBuilder()
            .WithName("Smartphone")
            .WithDescription("Latest smartphone")
            .WithPrice(999.99m)
            .WithStock(100)
            .WithCategories(Guid.NewGuid())
            .Build();

        // remove authorization
        _fixture.ClearCookies();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/products", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidCategory_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var request = new ProductRequestBuilder()
            .WithName("Smartphone 2")
            .WithDescription("Latest smartphone")
            .WithPrice(999.99m)
            .WithStock(100)
            .WithCategories(Guid.NewGuid()) // Invalid category ID
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/products", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturnUnprocessableEntity()
    {
        // Arrange
        await AuthenticateAdmin();
        var request = new ProductRequestBuilder()
            .WithName("") // Invalid name
            .WithDescription("Latest smartphone")
            .WithPrice(-100) // Invalid price
            .WithStock(-10) // Invalid stock
            .WithCategories(Guid.NewGuid())
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/products", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateAdmin();

        // Create category first
        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Electronics Category")
            .WithDescription("Electronic products")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter() },
        });

        var categoryId = categories![0].Id;

        var createRequest = new ProductRequestBuilder().Build();
        await _client.PostAsJsonAsync($"{_baseUrl}/products", createRequest);

        var productsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter() },
        });

        var productId = products![0].Id;

        var updateRequest = new UpdateProductRequestBuilder()
            .WithName("Updated Product")
            .WithDescription("Updated Description")
            .WithPrice(200)
            .WithStock(50)
            .WithImageUrl("http://example.com/updated.jpg")
            .WithStatus(ProductStatusDTO.Inactive)
            .WithCategories([categoryId])
            .Build();

        // Act
        var response = await _client.PutAsJsonAsync($"{_baseUrl}/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var updateRequest = new UpdateProductRequestBuilder().Build();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsJsonAsync($"{_baseUrl}/products/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("", "Valid description", 100, 10)]           // Invalid name
    [InlineData("Product", "Valid description", -10, 10)]    // Invalid price
    [InlineData("Product", "Valid description", 100, -5)]    // Invalid stock
    [InlineData("", "Valid description", -1, -1)]                         // Invalid name and price
    public async Task UpdateProduct_WithInvalidData_ShouldReturnUnprocessableEntity(
        string name,
        string description,
        decimal price,
        int stock)
    {
        // Arrange
        await AuthenticateAdmin();

        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Electronics Category")
            .WithDescription("Electronic products")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter() },
        });

        var categoryId = categories![0].Id;

        var createRequest = new ProductRequestBuilder()
            .WithCategories(categoryId)
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products", createRequest);

        var productsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter() },
        });

        var productId = products![0].Id;

        var updateRequest = new UpdateProductRequestBuilder()
            .WithName(name)
            .WithDescription(description)
            .WithPrice(price)
            .WithStock(stock)
            .WithCategories([categoryId])
            .Build();

        // Act
        var response = await _client.PutAsJsonAsync($"{_baseUrl}/products/{productId}", updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task DeleteProduct_AsAdmin_ShouldDeleteSuccessfully()
    {
        // Arrange
        await AuthenticateAdmin();

        var categoryRequest = new CategoryRequestBuilder()
            .WithName("Electronics Category")
            .WithDescription("Electronic products")
            .Build();

        await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringSetConverter() },
        });

        var categoryId = categories![0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName("Smartphone X")
            .WithPrice(2500)
            .WithStock(10)
            .WithCategories(categoryId)
            .Build();

        var createResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var productsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<ProductResponseDTO>() },
        });
        var productId = products!.First().Id;

        // Act
        var deleteResponse = await _client.DeleteAsync($"{_baseUrl}/products/{productId}");

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deleted = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        deleted.ShouldBeTrue();

        var finalProductsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var finalProducts = await finalProductsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonReadOnlyListConverter<ProductResponseDTO>() },
        });
        finalProducts.ShouldBeEmpty();
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"{_baseUrl}/products/{invalidId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_Unauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _fixture.ClearCookies();

        var randomId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"{_baseUrl}/products/{randomId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ResetAsync();
}