using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.UnitTests.ValueObjects.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.ValueObjects;

public class MoneyTests
{
    private readonly MoneyBuilder _builder;

    public MoneyTests()
    {
        _builder = new MoneyBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var code = "USD";
        var amount = 100.00m;

        // Act
        var money = _builder.WithCode(code).WithAmount(amount).Build();

        // Assert
        money.Code.ShouldBe(code);
        money.Amount.ShouldBe(amount);
    }

    [Fact]
    public void Create_ShouldFail_WhenInvalidCurrencyCodeProvided()
    {
        // Arrange
        var code = "INVALID";
        var amount = 100.00m;

        // Act
        Should.Throw<DomainException>(() => _builder.WithCode(code).WithAmount(amount).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_INVALID_CURRENCY"
        && error.Description == "Invalid currency code" && error.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenAmountIsZero()
    {
        // Arrange
        var code = "USD";
        var amount = 0.00m;

        // Act
        Should.Throw<DomainException>(() => _builder.WithCode(code).WithAmount(amount).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_AMOUNT_GT_0"
        && error.Description == "Amount must be greater than 0" && error.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenAmountIsNegative()
    {
        // Arrange
        var code = "USD";
        var amount = -100.00m;

        // Act
        Should.Throw<DomainException>(() => _builder.WithCode(code).WithAmount(amount).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_AMOUNT_GT_0"
        && error.Description == "Amount must be greater than 0" && error.ErrorType == ErrorType.Validation);
    }
}