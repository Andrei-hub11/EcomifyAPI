using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.Domain.ValueObjects;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.Entities;

public class ProductTests
{
    private readonly ProductBuilder _builder;

    public ProductTests()
    {
        _builder = new ProductBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Act
        var result = _builder.Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("Test Product");
        result.Value.Price.Amount.ShouldBe(100.00m);
    }

    [Theory]
    [InlineData("", "Valid description", 100, 10, "https://img.com/img.png", ProductStatusEnum.Active, "ERR_NAME_REQ")]
    [InlineData("Valid name", "", 100, 10, "https://img.com/img.png", ProductStatusEnum.Active, "ERR_DESC_REQ")]
    [InlineData("Valid name", "Valid description", 0, 10, "https://img.com/img.png", ProductStatusEnum.Active, "ERR_PRICE_GT0")]
    [InlineData("Valid name", "Valid description", 100, -1, "https://img.com/img.png", ProductStatusEnum.Active, "ERR_STOCK_GT0")]
    [InlineData("Valid name", "Valid description", 100, 10, "", ProductStatusEnum.Active, "ERR_IMG_REQ")]
    [InlineData("Valid name", "Valid description", 100, 10, "https://img.com/img.png", ProductStatusEnum.Inactive, "ERR_STATUS_ACTIVE")]
    public void ValidateProduct_ShouldReturnExpectedError(
           string name,
           string description,
           decimal price,
           int stock,
           string imageUrl,
           ProductStatusEnum status,
           string expectedErrorCode)
    {
        // Act
        var result = _builder.WithName(name)
            .WithDescription(description)
            .WithPrice(price)
            .WithStock(stock)
            .WithImageUrl(imageUrl)
            .WithStatus(status)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == expectedErrorCode);
    }

    [Theory]
    [InlineData("", "ERR_CURRENCY_REQ", "Currency code is required")]
    [InlineData("INVALID", "ERR_INVALID_CURRENCY", "Invalid currency code")]
    public void Create_ShouldFail_WhenCurrencyIsInvalid(string currencyCode, string expectedErrorCode, string expectedDescription)
    {
        // Act
        Should.Throw<DomainException>(() =>
            _builder
            .WithCurrencyCode(currencyCode)
            .Build()).Errors.ShouldContain(e => e.Code == expectedErrorCode
            && e.Description == expectedDescription && e.ErrorType == ErrorType.Validation);

    }


    [Theory]
    [InlineData("", "Valid description", 100, 10, "https://img.com/img.png", ProductStatusEnum.Active, "ERR_NAME_REQ")]
    [InlineData("Valid name", "", 100, 10, "https://img.com/img.png", ProductStatusEnum.Active, "ERR_DESC_REQ")]
    [InlineData("Valid name", "Valid description", 0, 10, "https://img.com/img.png", ProductStatusEnum.Active, "ERR_PRICE_GT0")]
    [InlineData("Valid name", "Valid description", 100, -1, "https://img.com/img.png", ProductStatusEnum.Active, "ERR_STOCK_GT0")]
    [InlineData("Valid name", "Valid description", 100, 10, "", ProductStatusEnum.Active, "ERR_IMG_REQ")]
    [InlineData("Valid name", "Valid description", 100, 10, "https://img.com/img.png", ProductStatusEnum.Inactive, "ERR_STATUS_ACTIVE")]
    public void From_ShouldReturnExpectedError(
        string name,
        string description,
        decimal price,
        int stock,
        string imageUrl,
        ProductStatusEnum status,
        string expectedErrorCode)
    {
        // Act
        var result = _builder.WithName(name)
            .WithDescription(description)
            .WithPrice(price)
            .WithStock(stock)
            .WithImageUrl(imageUrl)
            .WithStatus(status)
            .BuildFrom();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == expectedErrorCode);
    }

    [Theory]
    [InlineData("", "ERR_CURRENCY_REQ", "Currency code is required")]
    [InlineData("INVALID", "ERR_INVALID_CURRENCY", "Invalid currency code")]
    public void From_ShouldFail_WhenCurrencyCodeIsEmpty(string currencyCode, string expectedErrorCode, string expectedDescription)
    {
        // Act
        Should.Throw<DomainException>(() =>
            _builder
            .WithCurrencyCode(currencyCode)
            .BuildFrom()).Errors.ShouldContain(e => e.Code == expectedErrorCode
            && e.Description == expectedDescription && e.ErrorType == ErrorType.Validation);

    }

    [Fact]
    public void DecrementStock_ShouldSucceed_WhenEnoughStock()
    {
        // Arrange
        var product = _builder
            .WithStock(10)
            .Build()
            .Value;

        // Act
        var result = product!.DecrementStock(5);

        // Assert
        result.ShouldBeTrue();
        product.Stock.ShouldBe(5);
    }

    [Fact]
    public void DecrementStock_ShouldFail_WhenNotEnoughStock()
    {
        // Arrange
        var product = _builder
            .WithStock(5)
            .Build()
            .Value;

        // Act
        var result = product!.DecrementStock(10);

        // Assert
        result.ShouldBeFalse();
        product.Stock.ShouldBe(5);
    }

    [Fact]
    public void UpdatePrice_ShouldSucceed_WhenValidPriceProvided()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act
        product!.UpdatePrice(150.00m, "BRL").ShouldBeTrue();

        // Assert
        product.Price.Amount.ShouldBe(150.00m);
        product.Price.Code.ShouldBe("BRL");
    }

    [Fact]
    public void UpdatePrice_ShouldThrow_WhenPriceIsZeroOrNegative()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act & Assert
        Should.Throw<DomainException>(() =>
            product!.UpdatePrice(0, "BRL")).Errors.ShouldContain(e =>
                e.Code == "ERR_PRICE_GT0" &&
                e.Description == "Price must be greater than 0" &&
                e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateProductStatus()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act
        product!.UpdateStatus(ProductStatusEnum.Inactive).ShouldBeTrue();

        // Assert
        product.Status.ShouldBe(ProductStatusEnum.Inactive);
    }

    [Fact]
    public void UpdateImageUrl_ShouldSucceed_WhenValidUrlProvided()
    {
        // Arrange
        var product = _builder.Build().Value;
        var newUrl = "https://example.com/image.png";

        // Act
        product!.UpdateImageUrl(newUrl).ShouldBeTrue();

        // Assert
        product.ImageUrl.ShouldBe(newUrl);
    }

    [Fact]
    public void UpdateImageUrl_ShouldThrow_WhenUrlIsEmpty()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act & Assert
        Should.Throw<DomainException>(() =>
            product!.UpdateImageUrl(string.Empty)).Errors.ShouldContain(e =>
                e.Code == "ERR_IMG_REQ" &&
                e.Description == "ImageUrl is required" &&
                e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void UpdateCategories_ShouldSucceed_WhenValidCategoriesProvided()
    {
        // Arrange
        var product = _builder.Build().Value;

        var categories = new List<ProductCategory>
        {
            new(product!.Id, Guid.NewGuid()),
            new(product.Id, Guid.NewGuid())
        };

        // Act
        product!.UpdateCategories(categories).ShouldBeTrue();

        // Assert
        product.ProductCategories.Count.ShouldBe(2);
    }

    [Fact]
    public void UpdateCategories_ShouldThrow_WhenListIsEmpty()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act & Assert
        Should.Throw<DomainException>(() =>
            product!.UpdateCategories([])).Errors.ShouldContain(e =>
                e.Code == "ERR_CAT_MIN1" &&
                e.Description == "Categories must be at least one" &&
                e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void UpdateCategories_ShouldThrow_WhenCategoriesAreNotUnique()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var product = _builder.Build().Value;
        var categories = new List<ProductCategory>
        {
            new(product!.Id, categoryId),
            new(product.Id, categoryId2)
        };

        // Simulate existing categories with same ID
        product!.UpdateCategories(categories);

        // Act & Assert
        Should.Throw<DomainException>(() =>
            product.UpdateCategories(
            [
                new(categoryId, product.Id),
                new(categoryId2, product.Id)
            ])).Errors.ShouldContain(e =>
                e.Code == "ERR_CAT_UNQ" &&
                e.Description == "Categories must be unique" &&
                e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void UpdateCategories_ShouldThrow_WhenCategoriesAreNotAssociatedWithProduct()
    {
        // Arrange
        var product = _builder.Build().Value;
        var categoryId = Guid.NewGuid();
        var categoryId2 = Guid.NewGuid();
        var categories = new List<ProductCategory>
        {
            new(Guid.NewGuid(), categoryId),
            new(Guid.NewGuid(), categoryId2)
        };

        // Act & Assert
        Should.Throw<DomainException>(() =>
            product!.UpdateCategories(categories)).Errors.ShouldContain(e =>
                e.Code == "ERR_CAT_ASSOC" &&
                e.Description == "Categories must be associated with the product" &&
                e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void UpdateStock_ShouldSucceed_WhenQuantityIsValid()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act
        product!.UpdateStock(25).ShouldBeTrue();

        // Assert
        product.Stock.ShouldBe(25);
    }

    [Fact]
    public void UpdateStock_ShouldThrow_WhenQuantityIsNegative()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act & Assert
        Should.Throw<DomainException>(() =>
            product!.UpdateStock(-1)).Errors.ShouldContain(e =>
                e.Code == "ERR_QUANTITY_GT0" &&
                e.Description == "Quantity must be greater than 0" &&
                e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void UpdateName_ShouldSucceed_WhenValidNameProvided()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act
        product!.UpdateName("New Name").ShouldBeTrue();

        // Assert
        product.Name.ShouldBe("New Name");
    }

    [Fact]
    public void UpdateName_ShouldThrow_WhenNameIsEmpty()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act & Assert
        Should.Throw<DomainException>(() =>
            product!.UpdateName(string.Empty)).Errors.ShouldContain(e =>
                e.Code == "ERR_NAME_REQ" &&
                e.Description == "Name is required" &&
                e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void UpdateDescription_ShouldSucceed_WhenValidDescriptionProvided()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act
        product!.UpdateDescription("New Description").ShouldBeTrue();

        // Assert
        product.Description.ShouldBe("New Description");
    }

    [Fact]
    public void UpdateDescription_ShouldThrow_WhenDescriptionIsEmpty()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act & Assert
        Should.Throw<DomainException>(() =>
            product!.UpdateDescription(string.Empty)).Errors.ShouldContain(e =>
                e.Code == "ERR_DESC_REQ" &&
                e.Description == "Description is required" &&
                e.ErrorType == ErrorType.Validation);
    }

}