using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.Entities;

public class CouponBehaviorTests
{
    private readonly DiscountBuilder _discountBuilder;

    public CouponBehaviorTests()
    {
        _discountBuilder = new DiscountBuilder();
    }

    [Theory]
    [MemberData(nameof(GetValidDiscountData))]
    public void Create_Should_Return_Valid_Discount(
        string code,
        DiscountType discountType,
        decimal? fixedAmount,
        decimal? percentage,
        int maxUses,
        decimal minOrder,
        int maxPerUser)
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var builder = _discountBuilder
            .WithCode(code)
            .WithDiscountType(discountType)
            .WithMaxUses(maxUses)
            .WithMinOrderAmount(minOrder)
            .WithMaxUsesPerUser(maxPerUser)
            .WithPercentage(percentage)
            .WithFixedAmount(fixedAmount)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo);

        // Act
        var result = builder.Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        var coupon = result.Value;

        coupon.Code.ShouldBe(code.ToUpperInvariant());
        coupon.DiscountType.ShouldBe(discountType);
        coupon.FixedAmount.ShouldBe(fixedAmount > 0 ? fixedAmount : null);
        coupon.Percentage.ShouldBe(percentage > 0 ? percentage : null);
        coupon.MaxUses.ShouldBe(maxUses);
        coupon.MinOrderAmount.ShouldBe(minOrder);
        coupon.MaxUsesPerUser.ShouldBe(maxPerUser);
        coupon.ValidFrom.ShouldBe(validFrom);
        coupon.ValidTo.ShouldBe(validTo);
        coupon.IsActive.ShouldBeTrue();
        coupon.Uses.ShouldBe(0);
    }

    public static IEnumerable<object[]> GetValidDiscountData()
    {
        yield return new object[] { "WELCOME10", DiscountType.Fixed, 10.0m, null!, 100, 0.0m, 1 };
        yield return new object[] { "OFF50", DiscountType.Percentage, null!, 50.0m, 5, 200.0m, 2 };
        yield return new object[] { "CODEBOTH", DiscountType.Coupon, 5.0m, null!, 10, 50.0m, 1 };
        yield return new object[] { "CODEBOTH", DiscountType.Coupon, null!, 2.0m, 10, 50.0m, 1 };
    }

    [Theory]
    [MemberData(nameof(GetInvalidDiscountData))]
    public void Create_Should_Fail_With_Invalid_Data(
    string code,
    DiscountType discountType,
    decimal? fixedAmount,
    decimal? percentage,
    int maxUses,
    decimal minOrder,
    int maxPerUser,
    int fromOffsetDays,
    int toOffsetDays,
    string expectedErrorCode)
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddDays(fromOffsetDays);
        var validTo = DateTime.UtcNow.AddDays(toOffsetDays);

        var builder = _discountBuilder
            .WithCode(code)
            .WithDiscountType(discountType)
            .WithMaxUses(maxUses)
            .WithMinOrderAmount(minOrder)
            .WithMaxUsesPerUser(maxPerUser)
            .WithPercentage(percentage)
            .WithFixedAmount(fixedAmount)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo);

        // Act
        var result = builder.Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == expectedErrorCode);
    }

    public static IEnumerable<object[]> GetInvalidDiscountData()
    {
        // Code validation only happens for Coupon type now
        yield return new object[] { "", DiscountType.Coupon, 10.0m, null!, 1, 0.0m, 1, 0, 5, "ERR_CODE_REQ" };
        yield return new object[] { "CODE", (DiscountType)99, 10.0m, null!, 1, 0.0m, 1, 0, 5, "ERR_TYPE_INV" };

        // Fixed discount must have amount > 0 and no percentage
        yield return new object[] { "CODE", DiscountType.Fixed, -5.0m, null!, 1, 0.0m, 1, 0, 5, "ERR_AMT_GT_0" };
        yield return new object[] { "CODE", DiscountType.Fixed, null!, null!, 1, 0.0m, 1, 0, 5, "ERR_AMT_GT_0" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, 50.0m, 1, 0.0m, 1, 0, 5, "ERR_PERC_INV" };

        // Percentage discount must have percentage between 0-100 and no fixed amount
        yield return new object[] { "CODE", DiscountType.Percentage, null!, -5.0m, 1, 0.0m, 1, 1, 5, "ERR_PERC_INV" };
        yield return new object[] { "CODE", DiscountType.Percentage, null!, null!, 1, 0.0m, 1, 1, 5, "ERR_PERC_INV" };
        yield return new object[] { "CODE", DiscountType.Percentage, null!, 101.0m, 1, 0.0m, 1, 1, 5, "ERR_PERC_INV" };
        yield return new object[] { "CODE", DiscountType.Percentage, 10.0m, 50.0m, 1, 0.0m, 1, 0, 5, "ERR_AMT_INV" };

        // Coupon must have either valid fixed amount or percentage
        yield return new object[] { "CODE", DiscountType.Coupon, null!, null!, 1, 0.0m, 1, 0, 5, "ERR_AMT_OR_PERC_REQ" };
        yield return new object[] { "CODE", DiscountType.Coupon, -5.0m, null!, 1, 0.0m, 1, 0, 5, "ERR_AMT_GT_0" };
        yield return new object[] { "CODE", DiscountType.Coupon, null!, -5.0m, 1, 0.0m, 1, 0, 5, "ERR_PERC_INV" };

        // General validation cases
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, null!, 0, 0.0m, 1, 0, 5, "ERR_MAXU" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, null!, 1, -1.0m, 1, 0, 5, "ERR_MIN_ORD" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, null!, 1, 0.0m, 0, 0, 5, "ERR_MAXU" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, null!, 1, 0.0m, 1, -2, 5, "ERR_DATE_INV" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, null!, 1, 0.0m, 1, 0, -1, "ERR_DATE_INV" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, null!, 1, 0.0m, 1, 5, 5, "ERR_DATE_INV" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, null!, 1, 0.0m, 1, 366, 370, "ERR_DATE_INV" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, null!, 1, 0.0m, 1, 0, 400, "ERR_DATE_INV" };
    }


    [Fact]
    public void Create_Should_Fail_When_Coupon_Has_No_Discount_Value()
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var result = _discountBuilder
            .WithCode("INVALID")
            .WithDiscountType(DiscountType.Coupon)
            .WithFixedAmount(null)
            .WithPercentage(null)
            .WithMaxUses(10)
            .WithMinOrderAmount(100)
            .WithMaxUsesPerUser(1)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.OfType<ValidationError>().ShouldContain(e => e.Field == "fixedAmount"
        && e.Description == "Either fixed amount or percentage must be provided");
        result.Errors.OfType<ValidationError>().ShouldContain(e => e.Field == "percentage"
        && e.Description == "Either fixed amount or percentage must be provided");
    }

    [Fact]
    public void From_Should_Return_Valid_Discount_When_Data_Is_Correct()
    {
        // Arrange
        var builder = _discountBuilder
            .WithCode("NEW10")
            .WithFixedAmount(10)
            .WithDiscountType(DiscountType.Fixed)
            .WithMaxUses(5)
            .WithMinOrderAmount(50)
            .WithMaxUsesPerUser(1)
            .WithPercentage(null)
            .WithUses(1);

        // Act
        var result = builder.BuildFrom();

        // Assert
        result.IsFailure.ShouldBeFalse();
        var coupon = result.Value;
        coupon.Code.ShouldBe("NEW10");
        coupon.IsActive.ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(GetInvalidCouponData))]
    public void From_Should_Fail_With_Invalid_Data(
    string code,
    DiscountType discountType,
    decimal amount,
    int maxUses,
    decimal minOrder,
    int maxPerUser,
    Guid id,
    string expectedErrorCode)
    {
        // Arrange
        var validFrom = DateTime.UtcNow;
        var validTo = DateTime.UtcNow.AddDays(5);

        // Act
        var result = _discountBuilder
            .WithCode(code)
            .WithDiscountType(discountType)
            .WithFixedAmount(amount)
            .WithMaxUses(maxUses)
            .WithMinOrderAmount(minOrder)
            .WithMaxUsesPerUser(maxPerUser)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .WithId(id)
            .BuildFrom();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == expectedErrorCode);
    }

    public static IEnumerable<object[]> GetInvalidCouponData()
    {
        yield return new object[] { "", DiscountType.Coupon, 10.0m, 1, 0.0m, 1, Guid.Empty, "ERR_CODE_REQ" };
        yield return new object[] { "CODE", (DiscountType)99, 10.0m, 1, 0.0m, 1, Guid.Empty, "ERR_TYPE_INV" };
        yield return new object[] { "CODE", DiscountType.Fixed, -5.0m, 1, 0.0m, 1, Guid.Empty, "ERR_AMT_GT_0" };
        yield return new object[] { "CODE", DiscountType.Fixed, null!, 1, 0.0m, 1, Guid.Empty, "ERR_AMT_GT_0" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, 0, 0.0m, 1, Guid.Empty, "ERR_MAXU" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, 1, -1.0m, 1, Guid.Empty, "ERR_MIN_ORD" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, 1, 0.0m, 0, Guid.Empty, "ERR_MAXU" };
        yield return new object[] { "CODE", DiscountType.Fixed, 10.0m, 1, 0.0m, 1, Guid.Empty, "ERR_ID_REQ" };
    }

    [Fact]
    public void IsValidForUse_Should_Return_True_When_All_Conditions_Are_Met()
    {
        // Arrange
        var result = _discountBuilder
            .WithUses(1)
            .WithMaxUses(10)
            .WithMinOrderAmount(50)
            .WithMaxUsesPerUser(3)
            .WithDiscountType(DiscountType.Coupon)
            .WithPercentage(null)
            .WithIsActive(true)
            .WithValidFrom(DateTime.UtcNow.AddMinutes(-5))
            .BuildFrom();

        result.IsFailure.ShouldBeFalse();

        // Assert
        result.Value!.IsValidForUse(100, 1).ShouldBeTrue();
    }

    [Theory]
    [MemberData(nameof(GetInvalidDiscountDataForUse))]
    public void IsValidForUse_Should_Return_False_When_Conditions_Not_Met(
       bool isActive, decimal orderAmount, int userUsage,
       DiscountType discountType, decimal? fixedAmount, decimal? percentage,
       DateTime validFrom, DateTime validTo)
    {
        // Arrange
        var result = _discountBuilder
            .WithIsActive(isActive)
            .WithUses(1)
            .WithMaxUses(5)
            .WithMinOrderAmount(50)
            .WithMaxUsesPerUser(2)
            .WithDiscountType(discountType)
            .WithFixedAmount(fixedAmount)
            .WithPercentage(percentage)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .BuildFrom();

        result.IsFailure.ShouldBeFalse();

        // Assert
        result.Value.IsValidForUse(orderAmount, userUsage).ShouldBeFalse();
    }


    public static IEnumerable<object[]> GetInvalidDiscountDataForUse()
    {
        yield return new object[] { false, 100, 1, DiscountType.Fixed, 10m, null!,
        DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(1) }; // Inactive
        yield return new object[] { true, 100, 5, DiscountType.Coupon, 10m, null!,
        DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(1) }; // Exceeded user usage
        yield return new object[] { true, 30, 1, DiscountType.Percentage, null!, 50m,
        DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddDays(1) }; // Below min order
        yield return new object[] { true, 100, 1, DiscountType.Fixed, 10m, null!,
        DateTime.UtcNow.AddMinutes(10), DateTime.UtcNow.AddDays(1) }; // Future start date (handled in test setup)
    }


    [Theory]
    [MemberData(nameof(GetDiscountCalculationData))]
    public void CalculateDiscount_Should_Return_Correct_Amount(
        DiscountType type,
        decimal? fixedAmount,
        decimal? percentage,
        decimal orderAmount,
        decimal expectedDiscount)
    {
        // Arrange
        var builder = _discountBuilder
            .WithDiscountType(type)
            .WithFixedAmount(fixedAmount)
            .WithPercentage(percentage)
            .WithUses(1)
            .WithMaxUses(10)
            .WithMinOrderAmount(0)
            .WithMaxUsesPerUser(5)
            .WithValidFrom(DateTime.UtcNow.AddMinutes(1))
            .WithValidTo(DateTime.UtcNow.AddDays(1))
            .WithIsActive(true);

        var result = builder.Build();

        result.IsFailure.ShouldBeFalse();

        // Act
        var discountAmount = result.Value.CalculateDiscount(orderAmount);

        // Assert
        discountAmount.ShouldBe(expectedDiscount);
    }

    public static IEnumerable<object[]> GetDiscountCalculationData()
    {
        yield return new object[] { DiscountType.Fixed, 10m, null!, 100m, 10m };
        yield return new object[] { DiscountType.Fixed, 10m, null!, 5m, 5m };
        yield return new object[] { DiscountType.Percentage, null!, 0.5m, 200m, 100m };
        yield return new object[] { DiscountType.Coupon, 20m, null!, 50m, 20m };
        yield return new object[] { DiscountType.Coupon, 100m, null!, 70m, 70m };
        yield return new object[] { DiscountType.Coupon, null!, 0.2m, 100m, 20m };
        yield return new object[] { DiscountType.Coupon, null!, 0.5m, 0m, 0m };
        yield return new object[] { DiscountType.Fixed, 5m, null!, 0m, 0m };
    }


    [Fact]
    public void IncrementUsage_Should_Increment_Uses()
    {
        // Arrange
        var result = _discountBuilder
            .WithUses(0)
            .WithMaxUses(3)
            .WithDiscountType(DiscountType.Coupon)
            .WithPercentage(null)
            .Build();

        // Act

        result.IsFailure.ShouldBeFalse();

        result.Value!.IncrementUsage();
        result.Value!.Uses.ShouldBe(1);

        result.Value!.IncrementUsage();
        result.Value!.Uses.ShouldBe(2);
    }

    [Fact]
    public void IncrementUsage_Should_Throw_When_Max_Uses_Reached()
    {
        var result = _discountBuilder
            .WithMaxUses(3)
            .WithDiscountType(DiscountType.Coupon)
            .WithPercentage(null)
            .Build();

        result.IsFailure.ShouldBeFalse();

        result.Value.IncrementUsage();
        result.Value.IncrementUsage();
        result.Value.IncrementUsage();

        Should.Throw<InvalidOperationException>(() => result.Value.IncrementUsage());
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_To_False()
    {
        // Arrange
        var result = _discountBuilder
            .WithIsActive(true)
            .WithDiscountType(DiscountType.Coupon)
            .WithPercentage(null)
            .Build();

        result.IsFailure.ShouldBeFalse();

        // Act
        result.Value.Deactivate();

        result.Value.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Create_Should_Fail_When_Fixed_Discount_Has_No_Amount()
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var result = _discountBuilder
            .WithCode("TEST")
            .WithDiscountType(DiscountType.Fixed)
            .WithFixedAmount(null)
            .WithPercentage(null)
            .WithMaxUses(10)
            .WithMinOrderAmount(100)
            .WithMaxUsesPerUser(1)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.OfType<ValidationError>().ShouldContain(e => e.Field == "fixedAmount"
            && e.Code == "ERR_AMT_GT_0");
    }

    [Fact]
    public void Create_Should_Fail_When_Percentage_Discount_Has_No_Percentage()
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var result = _discountBuilder
            .WithCode("TEST")
            .WithDiscountType(DiscountType.Percentage)
            .WithFixedAmount(null)
            .WithPercentage(null)
            .WithMaxUses(10)
            .WithMinOrderAmount(100)
            .WithMaxUsesPerUser(1)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.OfType<ValidationError>().ShouldContain(e => e.Field == "percentage"
            && e.Code == "ERR_PERC_INV");
    }

    [Fact]
    public void Create_Should_Succeed_With_Valid_Fixed_Discount()
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var result = _discountBuilder
            .WithCode("FIXED10")
            .WithDiscountType(DiscountType.Fixed)
            .WithFixedAmount(10m)
            .WithPercentage(null)
            .WithMaxUses(10)
            .WithMinOrderAmount(0)
            .WithMaxUsesPerUser(1)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.DiscountType.ShouldBe(DiscountType.Fixed);
        result.Value.FixedAmount.ShouldBe(10m);
        result.Value.Percentage.ShouldBeNull();
    }

    [Fact]
    public void Create_Should_Succeed_With_Valid_Percentage_Discount()
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var result = _discountBuilder
            .WithCode("PERCENT20")
            .WithDiscountType(DiscountType.Percentage)
            .WithFixedAmount(null)
            .WithPercentage(20m)
            .WithMaxUses(10)
            .WithMinOrderAmount(0)
            .WithMaxUsesPerUser(1)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.DiscountType.ShouldBe(DiscountType.Percentage);
        result.Value.FixedAmount.ShouldBeNull();
        result.Value.Percentage.ShouldBe(20m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Create_Should_Fail_When_Coupon_Has_Invalid_Fixed_Amount(decimal amount)
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var result = _discountBuilder
            .WithCode("COUPON")
            .WithDiscountType(DiscountType.Coupon)
            .WithFixedAmount(amount)
            .WithPercentage(null)
            .WithMaxUses(10)
            .WithMinOrderAmount(0)
            .WithMaxUsesPerUser(1)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.OfType<ValidationError>().ShouldContain(e => e.Field == "fixedAmount"
            && e.Code == "ERR_AMT_GT_0");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void Create_Should_Fail_When_Coupon_Has_Invalid_Percentage(decimal percentage)
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var result = _discountBuilder
            .WithCode("COUPON")
            .WithDiscountType(DiscountType.Coupon)
            .WithFixedAmount(null)
            .WithPercentage(percentage)
            .WithMaxUses(10)
            .WithMinOrderAmount(0)
            .WithMaxUsesPerUser(1)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.OfType<ValidationError>().ShouldContain(e => e.Field == "percentage"
            && e.Code == "ERR_PERC_INV");
    }

    [Fact]
    public void Create_Should_Succeed_When_Coupon_Has_Both_Fixed_And_Percentage()
    {
        // Arrange
        var validFrom = DateTime.UtcNow.AddMinutes(1);
        var validTo = DateTime.UtcNow.AddDays(7);

        var result = _discountBuilder
            .WithCode("BOTHVALID")
            .WithDiscountType(DiscountType.Coupon)
            .WithFixedAmount(15m)
            .WithPercentage(10m)
            .WithMaxUses(10)
            .WithMinOrderAmount(0)
            .WithMaxUsesPerUser(1)
            .WithValidFrom(validFrom)
            .WithValidTo(validTo)
            .Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.DiscountType.ShouldBe(DiscountType.Coupon);
        result.Value.FixedAmount.ShouldBe(15m);
        result.Value.Percentage.ShouldBe(10m);
    }
}