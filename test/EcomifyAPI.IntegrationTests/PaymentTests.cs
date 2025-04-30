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
public class PaymentTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly AppHostFixture _fixture;
    private readonly string _baseUrl = "https://localhost:7037/api/v1";
    private readonly ITestOutputHelper _output;
    private string _userId = string.Empty;
    private string _adminId = string.Empty;
    private Guid _categoryId = Guid.Empty;
    private Guid _productId = Guid.Empty;
    private Guid _cartId = Guid.Empty;
    private Guid _transactionId = Guid.Empty;

    public PaymentTests(AppHostFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _client = fixture.CreateClient();
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

    private async Task<Guid> CreateProductWithCategory()
    {
        await AuthenticateAdmin();
        var categoryRequest = new CategoryRequestBuilder()
            .WithName($"Test Category {Guid.NewGuid()}")
            .WithDescription("Test Category for payment tests")
            .Build();

        var categoryResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products/categories", categoryRequest);
        categoryResponse.EnsureSuccessStatusCode();

        var categoriesResponse = await _client.GetAsync($"{_baseUrl}/products/categories");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<CategoryResponseDTO>() },
            });

        _categoryId = categories![0].Id;

        var productRequest = new ProductRequestBuilder()
            .WithName($"Test Product {Guid.NewGuid()}")
            .WithDescription("Test product for payment tests")
            .WithPrice(100.00m)
            .WithStock(10)
            .WithCategories(_categoryId)
            .Build();

        var productResponse = await _client.PostAsJsonAsync($"{_baseUrl}/products", productRequest);
        productResponse.EnsureSuccessStatusCode();

        var productsResponse = await _client.GetAsync($"{_baseUrl}/products");
        var products = await productsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProductResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<ProductResponseDTO>() },
            });

        return products![0].Id;
    }

    private async Task AddProductToCart(Guid productId)
    {
        // First get the cart to ensure it exists
        var cartResponse = await _client.GetAsync($"{_baseUrl}/carts/{_userId}");
        cartResponse.EnsureSuccessStatusCode();

        var addItemRequest = new AddItemRequestBuilder()
            .WithProductId(productId)
            .WithQuantity(1)
            .Build();

        var addItemResponse = await _client.PostAsJsonAsync($"{_baseUrl}/carts/{_userId}", addItemRequest);
        addItemResponse.EnsureSuccessStatusCode();

        var cart = await addItemResponse.Content.ReadFromJsonAsync<CartResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<CartItemResponseDTO>() },
            });

        _cartId = cart!.Id;
    }

    [Fact]
    public async Task GetPayments_WhenAdmin_ShouldReturnPayments()
    {
        // Arrange
        await AuthenticateAdmin();
        var filter = new PaymentFilterRequestBuilder()
            .WithPageSize(10)
            .WithPageNumber(1)
            .Build();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/payments?PageSize={filter.PageSize}&PageNumber={filter.PageNumber}");
        var payments = await response.Content.ReadFromJsonAsync<PaginatedResponseDTO<PaymentResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        payments.ShouldNotBeNull();
        payments.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetPayments_WhenNotAdmin_ShouldReturnUnauthorized()
    {
        // Arrange
        await AuthenticateUser();
        var filter = new PaymentFilterRequestBuilder()
            .WithPageSize(10)
            .WithPageNumber(1)
            .Build();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/payments?PageSize={filter.PageSize}&PageNumber={filter.PageNumber}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPaymentByTransactionId_WithValidId_ShouldReturnPayment()
    {
        // Arrange - First create a payment
        await ProcessPayment_WithCreditCard_ShouldCreatePayment();
        await AuthenticateAdmin();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/payments/{_transactionId}/details");
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        payment.ShouldNotBeNull();
        payment.TransactionId.ShouldBe(_transactionId);
    }

    [Fact]
    public async Task GetPaymentByTransactionId_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/payments/{invalidId}/details");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaymentsByCustomerId_WithValidId_ShouldReturnPayments()
    {
        // Arrange - First create a payment
        await ProcessPayment_WithCreditCard_ShouldCreatePayment();

        // Act
        var response = await _client.GetAsync($"{_baseUrl}/payments/{_userId}/user");
        var payments = await response.Content.ReadFromJsonAsync<IReadOnlyList<PaymentResponseDTO>>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter(), new JsonReadOnlyListConverter<PaymentResponseDTO>() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        payments.ShouldNotBeNull();
        payments.Count.ShouldBeGreaterThanOrEqualTo(1);
        payments[0].StatusHistory.ShouldNotBeNull();
        payments[0].StatusHistory.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ProcessPayment_WithCreditCard_ShouldCreatePayment()
    {
        // Arrange
        _productId = await CreateProductWithCategory();
        await AuthenticateUser();
        await AddProductToCart(_productId);

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

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/payments/process", paymentRequest);
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        payment.ShouldNotBeNull();
        payment.PaymentMethod.ShouldBe(PaymentMethodEnumDTO.CreditCard);
        payment.Status.ShouldBe(PaymentStatusDTO.Succeeded);
        payment.CcLastFourDigits.ShouldBe("1111");
        payment.CcBrand.ShouldBe("Visa");
        payment.TransactionId.ShouldNotBe(Guid.Empty);

        // Save transaction ID for other tests
        _transactionId = payment.TransactionId;
    }

    [Fact]
    public async Task ProcessPayment_WithPayPal_ShouldCreatePayment()
    {
        // Arrange
        _productId = await CreateProductWithCategory();
        await AuthenticateUser();
        await AddProductToCart(_productId);

        var payPalDetails = new PayPalDetailsBuilder()
            .WithPayerId(Guid.NewGuid())
            .WithPayerEmail("test@example.com")
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
            .WithPaymentMethod(PaymentMethodEnumDTO.PayPal)
            .WithPayPalDetails(payPalDetails)
            .WithShippingAddress(shippingAddress)
            .WithBillingAddress(billingAddress)
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/payments/process", paymentRequest);
        var payment = await response.Content.ReadFromJsonAsync<PaymentResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        payment.ShouldNotBeNull();
        payment.PaymentMethod.ShouldBe(PaymentMethodEnumDTO.PayPal);
        payment.Status.ShouldBe(PaymentStatusDTO.Succeeded);
        payment.PaypalEmail.ShouldBe("test@example.com");
        payment.TransactionId.ShouldNotBe(Guid.Empty);

        // Save transaction ID for other tests
        _transactionId = payment.TransactionId;
    }

    [Fact]
    public async Task ProcessPayment_WithEmptyCart_ShouldReturnConflict()
    {
        // Arrange
        await AuthenticateUser();
        // Skip adding product to cart

        var creditCardDetails = new CreditCardDetailsBuilder().Build();
        var shippingAddress = new AddressRequestBuilder().Build();
        var billingAddress = new AddressRequestBuilder().Build();

        var paymentRequest = new PaymentRequestBuilder()
            .WithUserId(_userId)
            .WithPaymentMethod(PaymentMethodEnumDTO.CreditCard)
            .WithCreditCardDetails(creditCardDetails)
            .WithShippingAddress(shippingAddress)
            .WithBillingAddress(billingAddress)
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/payments/process", paymentRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    //[Fact]
    //public async Task ProcessPayment_WithInvalidPaymentMethod_ShouldReturnError()
    //{
    //    // Arrange
    //    _productId = await CreateProductWithCategory();
    //    await AuthenticateUser();
    //    await AddProductToCart(_productId);

    //    // Missing payment details
    //    var shippingAddress = new AddressRequestBuilder().Build();
    //    var billingAddress = new AddressRequestBuilder().Build();
    //    var creditCardDetails = new CreditCardDetailsBuilder().Build();

    //    var paymentRequest = new PaymentRequestBuilder()
    //       .WithUserId(_userId)
    //       .WithPaymentMethod((PaymentMethodEnumDTO)3)
    //       .WithCreditCardDetails(creditCardDetails)
    //       .WithShippingAddress(shippingAddress)
    //       .WithBillingAddress(billingAddress)
    //       .Build();

    //    // Act
    //    var response = await _client.PostAsJsonAsync($"{_baseUrl}/payments/process", paymentRequest);

    //    // Assert
    //    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    //}

    [Fact]
    public async Task ProcessPayment_WithWrongUserId_ShouldReturnConflict()
    {
        // Arrange
        _productId = await CreateProductWithCategory();
        await AuthenticateUser();
        await AddProductToCart(_productId);
        await AuthenticateAdmin();

        var creditCardDetails = new CreditCardDetailsBuilder().Build();
        var shippingAddress = new AddressRequestBuilder().Build();
        var billingAddress = new AddressRequestBuilder().Build();

        // Using a different user ID than the authenticated one
        var paymentRequest = new PaymentRequestBuilder()
            .WithUserId(_userId) // Will be wrong, because the authenticated user is the admin
            .WithPaymentMethod(PaymentMethodEnumDTO.CreditCard)
            .WithCreditCardDetails(creditCardDetails)
            .WithShippingAddress(shippingAddress)
            .WithBillingAddress(billingAddress)
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync($"{_baseUrl}/payments/process", paymentRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RefundPayment_WithValidId_ShouldRefundPayment()
    {
        // Arrange - First create a payment
        await ProcessPayment_WithCreditCard_ShouldCreatePayment();
        await AuthenticateAdmin();

        // Act
        var response = await _client.PostAsync($"{_baseUrl}/payments/{_transactionId}/refund", null);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldBeTrue();

        // Verify refund
        var getResponse = await _client.GetAsync($"{_baseUrl}/payments/{_transactionId}/details");
        var payment = await getResponse.Content.ReadFromJsonAsync<PaymentResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        payment!.Status.ShouldBe(PaymentStatusDTO.Refunded);
    }

    [Fact]
    public async Task RefundPayment_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"{_baseUrl}/payments/{invalidId}/refund", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelPayment_WithValidId_ShouldCancelPayment()
    {
        // Arrange - First create a payment
        await ProcessPayment_WithCreditCard_ShouldCreatePayment();
        await AuthenticateAdmin();

        // Act
        var response = await _client.PostAsync($"{_baseUrl}/payments/{_transactionId}/cancel", null);
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldBeTrue();

        // Verify cancellation
        var getResponse = await _client.GetAsync($"{_baseUrl}/payments/{_transactionId}/details");
        var payment = await getResponse.Content.ReadFromJsonAsync<PaymentResponseDTO>(
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringSetConverter() },
            });

        payment!.Status.ShouldBe(PaymentStatusDTO.Cancelled);
    }

    [Fact]
    public async Task CancelPayment_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateAdmin();
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"{_baseUrl}/payments/{invalidId}/cancel", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _fixture.ResetAsync();
}