using EcomifyAPI.Domain.Enums;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.Entities;

public class DiscountHistoryTests
{
    private readonly DiscountHistoryBuilder _builder;

    public DiscountHistoryTests()
    {
        _builder = new DiscountHistoryBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange & Act
        var result = _builder.AsFixedDiscount().Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.OrderId.ShouldNotBe(Guid.Empty);
        result.Value.CustomerId.ShouldBe("customer123");
        result.Value.DiscountType.ShouldBe(DiscountType.Fixed);
        result.Value.FixedAmount.ShouldBe(10m);
        result.Value.Percentage.ShouldBeNull();
        result.Value.AppliedAt.ShouldNotBe(default);
    }

    [Fact]
    public void Create_ShouldSucceed_WithCouponDiscount()
    {
        // Arrange & Act
        var result = _builder.AsCouponDiscount().Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.DiscountType.ShouldBe(DiscountType.Coupon);
        result.Value.CouponCode.ShouldBe("DISCOUNT10");
        result.Value.FixedAmount.ShouldBe(10m);
        result.Value.Percentage.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldSucceed_WithPercentageDiscount()
    {
        // Arrange & Act
        var result = _builder.AsPercentageDiscount().Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.DiscountType.ShouldBe(DiscountType.Percentage);
        result.Value.Percentage.ShouldBe(15m);
        result.Value.FixedAmount.ShouldBeNull();
    }

    [Fact]
    public void Create_ShouldFail_WhenOrderIdIsEmpty()
    {
        // Arrange & Act
        var result = _builder.WithEmptyOrderId().Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_ORDER_ID_REQUIRED");
    }

    [Fact]
    public void Create_ShouldFail_WhenCustomerIdIsEmpty()
    {
        // Arrange & Act
        var result = _builder.WithEmptyCustomerId().Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_CUSTOMER_ID_REQUIRED");
    }

    [Fact]
    public void Create_ShouldFail_WhenDiscountIdIsEmpty()
    {
        // Arrange & Act
        var result = _builder.WithEmptyDiscountId().Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_DISCOUNT_ID_REQUIRED");
    }

    [Fact]
    public void Create_ShouldFail_WhenFixedDiscountHasNoAmount()
    {
        // Arrange & Act
        var result = _builder
            .WithDiscountType(DiscountType.Fixed)
            .WithFixedAmount(null)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_AMT_GT_0");
    }

    [Fact]
    public void Create_ShouldFail_WhenPercentageDiscountHasNoPercentage()
    {
        // Arrange & Act
        var result = _builder
            .WithDiscountType(DiscountType.Percentage)
            .WithPercentage(null)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_PERC_INV");
    }

    [Fact]
    public void Create_ShouldFail_WhenPercentageDiscountHasFixedAmount()
    {
        // Arrange & Act
        var result = _builder
            .WithDiscountType(DiscountType.Percentage)
            .WithPercentage(15m)
            .WithFixedAmount(10m)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_AMT_INV");
    }

    [Fact]
    public void Create_ShouldFail_WhenFixedDiscountHasPercentage()
    {
        // Arrange & Act
        var result = _builder
            .WithDiscountType(DiscountType.Fixed)
            .WithFixedAmount(10m)
            .WithPercentage(15m)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_PERC_INV");
    }

    [Fact]
    public void Create_ShouldFail_WhenCouponDiscountHasNoCouponCode()
    {
        // Arrange & Act
        var result = _builder
            .WithDiscountType(DiscountType.Coupon)
            .WithCouponCode(string.Empty)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_CODE_REQ");
    }

    [Fact]
    public void Create_ShouldFail_WhenCouponDiscountHasNoFixedAmountOrPercentage()
    {
        // Arrange & Act
        var result = _builder
            .WithDiscountType(DiscountType.Coupon)
            .WithCouponCode("DISCOUNT10")
            .WithFixedAmount(null)
            .WithPercentage(null)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_AMT_OR_PERC_REQ");
    }

    [Fact]
    public void Create_ShouldFail_WhenDiscountAmountIsNegative()
    {
        // Arrange & Act
        var result = _builder
            .WithDiscountAmount(-10m)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_DISCOUNT_AMOUNT_NEGATIVE");
    }

    [Fact]
    public void From_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange & Act
        var result = _builder.BuildFromFactory();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.OrderId.ShouldNotBe(Guid.Empty);
        result.Value.CustomerId.ShouldBe("customer123");
        result.Value.DiscountType.ShouldBe(DiscountType.Fixed);
        result.Value.FixedAmount.ShouldBe(10m);
    }
}