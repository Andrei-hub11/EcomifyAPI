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
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenStreetIsInvalid(string? street)
    {
        // Act
        Should.Throw<ArgumentException>(() => _builder.WithStreet(street).Build())
        .Message.ShouldBe("Street is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenCityIsInvalid(string? city)
    {
        // Act
        Should.Throw<ArgumentException>(() => _builder.WithCity(city).Build())
        .Message.ShouldBe("City is required");
    }
}