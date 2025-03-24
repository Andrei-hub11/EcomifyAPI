using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Domain.Entities;
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
        result.Value.Status.ShouldBe(OrderStatusEnum.Created);
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
    public void ProcessPayment_ShouldUpdateStatus_WhenOrderIsCreated()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();
        order!.AddItem(product, 1, new Currency("USD", 100));

        // Act
        order.ProcessPayment();

        // Assert
        order.Status.ShouldBe(OrderStatusEnum.Processing);
    }

    [Fact]
    public void ProcessPayment_ShouldThrow_WhenOrderIsNotInCreatedStatus()
    {
        // Arrange
        var order = _builder
            .WithStatus(OrderStatusEnum.Processing)
            .Build()
            .Value;

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => order!.ProcessPayment());
    }

    [Fact]
    public void AddItem_ShouldSucceed_WhenOrderIsCreated()
    {
        // Arrange
        var order = _builder.Build().Value;
        var product = CreateSampleProduct();

        // Act
        order!.AddItem(product, 1, new Currency("USD", 100));

        // Assert
        order.OrderItems.Count.ShouldBe(1);
        order.TotalAmount.Amount.ShouldBe(100);
    }

    private static Product CreateSampleProduct()
    {
        var result = Product.Create(
            Guid.NewGuid(),
            "Sample Product",
            "Description",
            100,
            10,
            "http://example.com/image.jpg",
            ProductStatusEnum.Active);

        return result.Value!;
    }
}