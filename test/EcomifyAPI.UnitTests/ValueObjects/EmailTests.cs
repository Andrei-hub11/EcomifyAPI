using EcomifyAPI.Domain.Exceptions;
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
    [InlineData("", "Email cannot be empty")]
    [InlineData(null, "Email cannot be empty")]
    [InlineData("   ", "Email cannot be empty")]
    [InlineData("notanemail", "Invalid email format")]
    [InlineData("missing@domain", "Invalid email format")]
    [InlineData("@nodomain.com", "Invalid email format")]
    public void Create_ShouldThrow_WhenInvalidEmailProvided(string? email, string errorMessage)
    {
        // Act & Assert
        Should.Throw<DomainException>(() =>
            new Email(email!))
        .Errors.ShouldContain(e => e.Description == errorMessage);
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