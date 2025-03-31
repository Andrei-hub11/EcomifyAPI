using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.Domain.ValueObjects;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.ValueObjects;

public class OrderItemTests
{
    private readonly OrderItemBuilder _builder;

    public OrderItemTests()
    {
        _builder = new OrderItemBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var orderItem = _builder.
        WithQuantity(1)
        .WithUnitPrice(new Money("USD", 100)).Build();

        // Assert
        orderItem.Id.ShouldNotBe(Guid.Empty);
        orderItem.ProductId.ShouldNotBe(Guid.Empty);
        orderItem.Quantity.ShouldBe(1);
        orderItem.UnitPrice.Amount.ShouldBe(100);
    }

    [Fact]
    public void Create_ShouldFail_WhenIdIsEmpty()
    {
        // Arrange
        Should.Throw<DomainException>(() => _builder.WithId(Guid.Empty).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_ID_REQUIRED"
        && e.Description == "Id is required" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenProductIdIsEmpty()
    {
        // Arrange
        Should.Throw<DomainException>(() => _builder.WithProductId(Guid.Empty).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_PRODUCT_ID_REQUIRED"
        && e.Description == "ProductId is required" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenQuantityIsZero()
    {
        // Arrange
        Should.Throw<DomainException>(() => _builder.WithQuantity(0).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_QTY_GT_0"
        && e.Description == "Quantity must be greater than 0" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenUnitPriceIsZero()
    {
        // Arrange
        Should.Throw<DomainException>(() => _builder.WithUnitPrice(new Money("USD", 0)).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_AMOUNT_GT_0"
        && e.Description == "Amount must be greater than 0" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenQuantityIsNegative()
    {
        // Arrange
        Should.Throw<DomainException>(() => _builder.WithQuantity(-1).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_QTY_GT_0"
        && e.Description == "Quantity must be greater than 0" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenUnitPriceIsNegative()
    {
        // Arrange
        Should.Throw<DomainException>(() => _builder.WithUnitPrice(new Money("USD", -1)).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_AMOUNT_GT_0"
        && e.Description == "Amount must be greater than 0" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void UpdateQuantity_ShouldSucceed_WhenQuantityIsPositive()
    {
        // Arrange
        var orderItem = _builder.
        WithQuantity(1)
        .WithUnitPrice(new Money("USD", 100)).Build();

        // Act
        orderItem.UpdateQuantity(2);

        // Assert
        orderItem.Quantity.ShouldBe(2);
    }

    [Fact]
    public void UpdateQuantity_ShouldFail_WhenQuantityIsZero()
    {
        // Arrange
        var orderItem = _builder.
        WithQuantity(1)
        .WithUnitPrice(new Money("USD", 100)).Build();

        // Act
        Should.Throw<DomainException>(() => orderItem.UpdateQuantity(0))
        .Errors.ShouldContain(e => e.Code == "ERR_QTY_GT_0"
        && e.Description == "Quantity must be greater than 0" && e.ErrorType == ErrorType.Validation);
    }
}