using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.ValueObjects;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.Entities;

public class CartTests
{
    private readonly CartBuilder _builder;

    public CartTests()
    {
        _builder = new CartBuilder();
    }

    [Fact]
    public void Cart_Should_Be_Created_Successfully()
    {
        var result = _builder.Build();
        result.IsFailure.ShouldBeFalse();
    }

    [Fact]
    public void Cart_Should_Be_Created_Successfully_With_Items()
    {
        var result = _builder.BuildFrom(
        [
            new(Guid.NewGuid(), 1, new Money("USD", 100))
        ]);

        result.IsFailure.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Cart_Should_Fail_When_UserId_Is_Empty(string? userId)
    {
        var result = _builder.WithUserId(userId!).Build();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(Error.Validation("User ID is required", "ERR_USER_ID_REQUIRED", "userId"));
    }

    [Fact]
    public void From_Should_Return_Cart_With_Properties_Expected()
    {
        var result = _builder.BuildFrom([new CartItemBuilder().Build()]);
        result.IsFailure.ShouldBeFalse();
        var cart = result.Value;
        cart.Id.ShouldNotBe(Guid.Empty);
        cart.UserId.ShouldNotBeEmpty();
        cart.CreatedAt.ShouldNotBe(DateTime.MinValue);
        cart.UpdatedAt.ShouldBeNull();
        cart.Items.ShouldNotBeNull();
        cart.Items.ShouldNotBeEmpty();
    }

    [Fact]
    public void From_Should_Fail_When_Data_Is_Invalid()
    {
        var result = _builder.WithId(Guid.Empty)
        .WithUserId("")
        .WithCreatedAt(DateTime.MinValue)
        .WithUpdatedAt(DateTime.MinValue)
        .BuildFrom([]);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Select(e => e.Code).ShouldContain("ERR_ID_REQUIRED");
        result.Errors.Select(e => e.Code).ShouldContain("ERR_USER_ID_REQUIRED");
        result.Errors.Select(e => e.Code).ShouldContain("ERR_CREATED_AT_REQUIRED");
    }

    [Fact]
    public void TotalAmount_ShouldBeZero_WhenCartHasNoItems()
    {
        // Arrange
        var result = _builder.BuildFrom([]);

        // Act
        result.IsFailure.ShouldBeFalse();
        var cart = result.Value;
        var totalAmount = cart.TotalAmount;

        // Assert
        totalAmount.Amount.ShouldBe(0);
        totalAmount.Code.ShouldBe("BRL");
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItem_WhenProductIdExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = new CartItem(productId, 1, new Money("BRL", 20));
        var result = _builder.BuildFrom(new List<CartItem> { item });
        result.IsFailure.ShouldBeFalse();
        var cart = result.Value;

        // Act
        cart.RemoveItem(productId);

        // Assert
        cart.Items.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveItem_ShouldThrow_WhenProductIdDoesNotExist()
    {
        // Arrange
        var existingItem = new CartItem(Guid.NewGuid(), 1, new Money("BRL", 20));
        var result = _builder.BuildFrom(new List<CartItem> { existingItem });
        result.IsFailure.ShouldBeFalse();
        var cart = result.Value;
        var nonExistingProductId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => cart.RemoveItem(nonExistingProductId));
    }

    [Fact]
    public void Clear_ShouldRemoveAllItemsFromCart()
    {
        // Arrange
        var items = new List<CartItem>
        {
            new(Guid.NewGuid(), 1, new Money("BRL", 10)),
            new(Guid.NewGuid(), 2, new Money("BRL", 15))
        };

        var result = _builder.BuildFrom(items);
        result.IsFailure.ShouldBeFalse();
        var cart = result.Value;

        // Act
        cart.Clear();

        // Assert
        cart.Items.ShouldBeEmpty();
    }

}