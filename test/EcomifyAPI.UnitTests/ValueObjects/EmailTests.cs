using EcomifyAPI.Domain.ValueObjects;

using Shouldly;

namespace EcomifyAPI.UnitTests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.com")]
    [InlineData("user@subdomain.domain.com")]
    public void Create_ShouldSucceed_WhenValidEmailProvided(string email)
    {
        // Act
        var result = new Email(email);

        // Assert
        result.Value.ShouldBe(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("notanemail")]
    [InlineData("missing@domain")]
    [InlineData("@nodomain.com")]
    public void Create_ShouldThrow_WhenInvalidEmailProvided(string? email)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            new Email(email));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenEmailsAreEqual()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act & Assert
        email1.Equals(email2).ShouldBeTrue();
        (email1 == email2).ShouldBeTrue();
    }
}