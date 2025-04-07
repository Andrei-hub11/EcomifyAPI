namespace EcomifyAPI.UnitTests.ValueObjects;

using System;

using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.Domain.ValueObjects;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

using Xunit;

public class CartItemTests
{
    private readonly CartItemBuilder _builder;

    public CartItemTests()
    {
        _builder = new CartItemBuilder();
    }

    [Fact]
    public void Should_Create_CartItem_When_ValidData()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var item = _builder.WithProductId(productId).WithQuantity(2).WithUnitPrice(new Money("BRL", 20)).Build();

        // Assert
        item.Id.ShouldNotBe(Guid.Empty);
        item.ProductId.ShouldBe(productId);
        item.Quantity.ShouldBe(2);
        item.UnitPrice.Amount.ShouldBe(20);
        item.TotalPrice.Amount.ShouldBe(40);
        item.TotalPrice.Code.ShouldBe("BRL");
    }

    [Fact]
    public void Should_Throw_When_ProductId_IsEmpty()
    {
        // Act
        var exception = Should.Throw<DomainException>(() =>
            _builder.WithProductId(Guid.Empty).WithQuantity(1).WithUnitPrice(new Money("BRL", 10)).Build()
        );

        // Assert
        exception.Errors.ShouldContain(e => e.Code == "ERR_PRODUCT_ID_REQUIRED");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Throw_When_Quantity_IsLessThanOrEqualToZero(int quantity)
    {
        var exception = Should.Throw<DomainException>(() =>
            new CartItem(Guid.NewGuid(), quantity, new Money("BRL", 10))
        );

        exception.Errors.ShouldContain(e => e.Code == "ERR_QTY_GT_0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Should_Throw_When_UnitPrice_IsLessThanOrEqualToZero(decimal unitPrice)
    {
        var exception = Should.Throw<DomainException>(() =>
            new CartItem(Guid.NewGuid(), 1, new Money("BRL", unitPrice))
        );

        exception.Errors.ShouldContain(e => e.Code == "ERR_AMOUNT_GT_0");
    }

    [Fact]
    public void Should_Throw_When_Id_IsEmpty()
    {
        var exception = Should.Throw<DomainException>(() =>
            new CartItem(Guid.NewGuid(), 1, new Money("BRL", 10), Guid.Empty)
        );

        exception.Errors.ShouldContain(e => e.Code == "ERR_ID_REQUIRED");
    }

    [Fact]
    public void UpdateQuantity_ShouldChangeQuantity_WhenValid()
    {
        // Arrange
        var item = new CartItemBuilder().WithQuantity(1).Build();

        // Act
        item.UpdateQuantity(5);

        // Assert
        item.Quantity.ShouldBe(5);
        item.TotalPrice.Amount.ShouldBe(item.UnitPrice.Amount * 5);
    }

    [Fact]
    public void UpdateQuantity_ShouldThrow_WhenInvalid()
    {
        var item = new CartItemBuilder().Build();

        var exception = Should.Throw<DomainException>(() => item.UpdateQuantity(0));

        exception.Errors.ShouldContain(e => e.Code == "ERR_QTY_GT_0");
    }

    [Fact]
    public void UpdateUnitPrice_ShouldChangePrice()
    {
        // Arrange
        var item = new CartItemBuilder().Build();
        var newPrice = new Money("BRL", 99.90m);

        // Act
        item.UpdateUnitPrice(newPrice);

        // Assert
        item.UnitPrice.ShouldBe(newPrice);
        item.TotalPrice.Amount.ShouldBe(newPrice.Amount * item.Quantity);
    }

    [Fact]
    public void TotalPrice_ShouldBeCalculatedCorrectly()
    {
        // Arrange
        var unitPrice = new Money("BRL", 7.5m);
        var item = new CartItem(Guid.NewGuid(), 4, unitPrice);

        // Act & Assert
        item.TotalPrice.Amount.ShouldBe(30m);
        item.TotalPrice.Code.ShouldBe("BRL");
    }
}