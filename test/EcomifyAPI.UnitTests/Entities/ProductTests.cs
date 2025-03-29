using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Domain.Exceptions;
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

    [Fact]
    public void Create_ShouldFail_WhenIdIsEmpty()
    {
        // Act
        var result = _builder
            .WithId(Guid.Empty)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_ID_REQUIRED");
    }

    [Fact]
    public void Create_ShouldFail_WhenPriceIsZero()
    {
        // Act
        var result = _builder
            .WithPrice(0)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_PRICE_MUST_BE_GREATER_THAN_0");
    }

    [Fact]
    public void Create_ShouldFail_WhenCurrencyCodeIsEmpty()
    {
        // Act
        Should.Throw<DomainException>(() =>
            _builder
            .WithCurrencyCode(string.Empty)
            .Build()).Errors.ShouldContain(e => e.Code == "ERR_CURRENCY_REQ"
            && e.Description == "Currency code is required" && e.ErrorType == ErrorType.Validation);

    }

    [Fact]
    public void Create_ShouldFail_WhenCurrencyCodeIsInvalid()
    {
        // Act
        Should.Throw<DomainException>(() =>
            _builder
            .WithCurrencyCode("INVALID")
            .Build()).Errors.ShouldContain(e => e.Code == "ERR_INVALID_CURRENCY"
            && e.Description == "Invalid currency code" && e.ErrorType == ErrorType.Validation);
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
        product!.UpdatePrice(150.00m, "BRL");

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
        Should.Throw<ArgumentException>(() =>
            product!.UpdatePrice(0, "BRL"));
    }

    [Fact]
    public void UpdateStatus_ShouldUpdateProductStatus()
    {
        // Arrange
        var product = _builder.Build().Value;

        // Act
        product!.UpdateStatus(ProductStatusEnum.Inactive);

        // Assert
        product.Status.ShouldBe(ProductStatusEnum.Inactive);
    }
}