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
        .WithUnitPrice(new Currency("USD", 100)).Build();

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
        Should.Throw<ArgumentException>(() => _builder.WithId(Guid.Empty).Build())
        .Message.ShouldBe("Id is required");
    }

    [Fact]
    public void Create_ShouldFail_WhenProductIdIsEmpty()
    {
        // Arrange
        Should.Throw<ArgumentException>(() => _builder.WithProductId(Guid.Empty).Build())
        .Message.ShouldBe("ProductId is required");
    }

    [Fact]
    public void Create_ShouldFail_WhenQuantityIsZero()
    {
        // Arrange
        Should.Throw<ArgumentException>(() => _builder.WithQuantity(0).Build())
        .Message.ShouldBe("Quantity must be greater than 0");
    }

    [Fact]
    public void Create_ShouldFail_WhenUnitPriceIsZero()
    {
        // Arrange
        Should.Throw<ArgumentException>(() => _builder.WithUnitPrice(new Currency("USD", 0)).Build())
        .Message.ShouldBe("Amount must be greater than 0");
    }

    [Fact]
    public void Create_ShouldFail_WhenQuantityIsNegative()
    {
        // Arrange
        Should.Throw<ArgumentException>(() => _builder.WithQuantity(-1).Build())
        .Message.ShouldBe("Quantity must be greater than 0");
    }

    [Fact]
    public void Create_ShouldFail_WhenUnitPriceIsNegative()
    {
        // Arrange
        Should.Throw<ArgumentException>(() => _builder.WithUnitPrice(new Currency("USD", -1)).Build())
        .Message.ShouldBe("Amount must be greater than 0");
    }

    [Fact]
    public void UpdateQuantity_ShouldSucceed_WhenQuantityIsPositive()
    {
        // Arrange
        var orderItem = _builder.
        WithQuantity(1)
        .WithUnitPrice(new Currency("USD", 100)).Build();

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
        .WithUnitPrice(new Currency("USD", 100)).Build();

        // Act
        Should.Throw<ArgumentException>(() => orderItem.UpdateQuantity(0))
        .Message.ShouldBe("Quantity must be greater than 0 (Parameter 'quantity')");
    }
}