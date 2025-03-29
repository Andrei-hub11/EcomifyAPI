using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.UnitTests.ValueObjects.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.ValueObjects;

public class AddressTests
{
    private readonly AddressBuilder _builder;

    public AddressTests()
    {
        _builder = new AddressBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Act
        var result = _builder.Build();

        // Assert
        result.Street.ShouldBe("123 Main St");
        result.City.ShouldBe("New York");
        result.State.ShouldBe("NY");
        result.ZipCode.ShouldBe("10001");
        result.Country.ShouldBe("United States");
        result.Complement.ShouldBe("Apt 4B");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenStreetIsInvalid(string? street)
    {
        // Act
        Should.Throw<DomainException>(() => _builder.WithStreet(street!).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_STREET_REQUIRED"
        && error.Description == "Street is required" && error.ErrorType == ErrorType.Validation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldFail_WhenNumberIsInvalid(int number)
    {
        // Act
        Should.Throw<DomainException>(() => _builder.WithNumber(number).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_NUMBER_GT_0"
        && error.Description == "Number must be greater than 0" && error.ErrorType == ErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenCityIsInvalid(string? city)
    {
        // Act
        Should.Throw<DomainException>(() => _builder.WithCity(city!).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_CITY_REQUIRED"
        && error.Description == "City is required" && error.ErrorType == ErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenStateIsInvalid(string? state)
    {
        // Act
        Should.Throw<DomainException>(() => _builder.WithState(state!).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_STATE_REQUIRED"
        && error.Description == "State is required" && error.ErrorType == ErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenZipCodeIsInvalid(string? zipCode)
    {
        // Act  
        Should.Throw<DomainException>(() => _builder.WithZipCode(zipCode!).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_ZIP_CODE_REQUIRED"
        && error.Description == "ZipCode is required" && error.ErrorType == ErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenCountryIsInvalid(string? country)
    {
        // Act
        Should.Throw<DomainException>(() => _builder.WithCountry(country!).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_COUNTRY_REQUIRED"
        && error.Description == "Country is required" && error.ErrorType == ErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenComplementIsInvalid(string? complement)
    {
        // Act  
        Should.Throw<DomainException>(() => _builder.WithComplement(complement!).Build())
        .Errors.ShouldContain(error => error.Code == "ERR_COMPLEMENT_REQUIRED"
        && error.Description == "Complement is required" && error.ErrorType == ErrorType.Validation);
    }
}