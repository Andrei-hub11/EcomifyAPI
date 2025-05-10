using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.Entities;

public class OrderTests
{
    private readonly OrderBuilder _builder;

    public OrderTests()
    {
        _builder = new OrderBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Act
        var result = _builder.Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.Status.ShouldBe(OrderStatusEnum.Confirmed);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenOrderStatusIsConfirmed()
    {
        // Act
        var result = _builder.WithStatus(OrderStatusEnum.Confirmed).Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.Status.ShouldBe(OrderStatusEnum.Confirmed);
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
    public void Create_ShouldFail_WhenUserIdIsEmpty()
    {
        // Act
        var result = _builder
            .WithUserId(string.Empty)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_USER_ID_REQUIRED");
    }

    [Fact]
    public void AddItem_ShouldSucceed_WhenOrderIsCreated()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();

        // Act
        order!.AddItem(product, 1, new Money("USD", 100));

        // Assert
        order.OrderItems.Count.ShouldBe(1);
        order.TotalAmount.Amount.ShouldBe(100);
    }

    [Fact]
    public void AddItem_ShouldSucceed_WhenOrderIsConfirmed()
    {
        // Arrange
        var order = _builder
            .WithStatus(OrderStatusEnum.Confirmed)
            .Build()
            .Value;
        var product = CreateSampleProduct();

        // Act
        order!.AddItem(product, 1, new Money("USD", 100));

        // Assert
        order.OrderItems.Count.ShouldBe(1);
        order.TotalAmount.Amount.ShouldBe(100);
    }

    [Fact]
    public void AddItem_ShouldUpdateExistingItem_WhenProductIdMatches()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();
        order!.AddItem(product, 1, new Money("USD", 100));

        // Act
        order.AddItem(product, 2, new Money("USD", 100));

        // Assert
        order.OrderItems.Count.ShouldBe(1);
        order.OrderItems[0].Quantity.ShouldBe(3);
        order.TotalAmount.Amount.ShouldBe(300);
    }

    [Fact]
    public void RemoveItem_ShouldSucceed_WhenOrderIsCreated()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();
        order!.AddItem(product, 1, new Money("USD", 100));

        // Act
        order.RemoveItem(product.Id);

        // Assert
        order.OrderItems.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveItem_ShouldSucceed_WhenOrderIsConfirmed()
    {
        // Arrange
        var order = _builder
            .WithStatus(OrderStatusEnum.Confirmed)
            .Build()
            .Value;
        var product = CreateSampleProduct();
        order!.AddItem(product, 1, new Money("USD", 100));

        // Act
        order.RemoveItem(product.Id);

        // Assert
        order.OrderItems.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveItem_ShouldFail_WhenOrderIsConfirmed()
    {
        // Arrange
        var order = _builder
            .WithStatus(OrderStatusEnum.Confirmed)
            .Build()
            .Value;
        var product = CreateSampleProduct();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => order!.RemoveItem(product.Id));
    }

    [Fact]
    public void ApplyDiscount_ShouldCorrectlyCalculateTotalWithDiscount()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();
        order!.AddItem(product, 2, new Money("USD", 100));

        // Act
        order.ApplyDiscount(50);

        // Assert
        order.TotalAmount.Amount.ShouldBe(200);
        order.DiscountAmount.ShouldBe(50);
        order.TotalWithDiscount.Amount.ShouldBe(150);
    }

    [Fact]
    public void ApplyDiscount_ShouldCapDiscountToTotalAmount_WhenDiscountIsGreaterThanTotal()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();
        order!.AddItem(product, 1, new Money("USD", 100));

        // Act
        order.ApplyDiscount(150);

        // Assert
        order.TotalAmount.Amount.ShouldBe(100);
        order.DiscountAmount.ShouldBe(100);
        order.TotalWithDiscount.Amount.ShouldBe(0);
    }

    [Fact]
    public void ApplyDiscount_ShouldThrow_WhenDiscountIsNegative()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();
        order!.AddItem(product, 1, new Money("USD", 100));

        // Act & Assert
        Should.Throw<ArgumentException>(() => order.ApplyDiscount(-10));
    }

    [Fact]
    public void TotalWithDiscount_ShouldBeUpdated_WhenAddingItems()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();
        order!.AddItem(product, 1, new Money("USD", 100));
        order.ApplyDiscount(20);

        // Initial check
        order.TotalAmount.Amount.ShouldBe(100);
        order.TotalWithDiscount.Amount.ShouldBe(80);

        // Act - add another item
        order.AddItem(product, 1, new Money("USD", 100));

        // Assert
        order.TotalAmount.Amount.ShouldBe(200);
        order.DiscountAmount.ShouldBe(20);
        order.TotalWithDiscount.Amount.ShouldBe(180);
    }

    [Fact]
    public void TotalWithDiscount_ShouldBeUpdated_WhenRemovingItems()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();
        var product2 = CreateSampleProduct();

        order!.AddItem(product, 1, new Money("USD", 100));
        order.AddItem(product2, 1, new Money("USD", 50));
        order.ApplyDiscount(30);

        // Initial check
        order.TotalAmount.Amount.ShouldBe(150);
        order.TotalWithDiscount.Amount.ShouldBe(120);

        // Act - remove an item
        order.RemoveItem(product2.Id);

        // Assert
        order.TotalAmount.Amount.ShouldBe(100);
        order.DiscountAmount.ShouldBe(30);
        order.TotalWithDiscount.Amount.ShouldBe(70);
    }

    [Fact]
    public void ShipOrder_ShouldSetShippedAtDate()
    {
        // Arrange
        var order = _builder.Build().Value;

        // Act
        order!.ShipOrder();

        // Assert
        order.ShippedAt.ShouldNotBeNull();
        order.ShippedAt.Value.Date.ShouldBe(DateTime.UtcNow.Date);
        order.Status.ShouldBe(OrderStatusEnum.Shipped);
    }

    [Fact]
    public void From_WithShippedAt_ShouldSetProperty()
    {
        // Arrange
        var shippedAt = DateTime.UtcNow;

        // Act
        var result = _builder
            .WithShippedAt(shippedAt)
            .BuildFrom();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.ShippedAt.ShouldBe(shippedAt);
    }

    private static Product CreateSampleProduct()
    {
        var result = Product.Create(
            "Sample Product",
            "Description",
            100,
            "BRL",
            10,
            "http://example.com/image.jpg",
            ProductStatusEnum.Active,
            Guid.NewGuid());

        return result.Value!;
    }
}