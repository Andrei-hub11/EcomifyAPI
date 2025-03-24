using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.Entities;

public class UserTests
{
    private readonly UserBuilder _builder;

    public UserTests()
    {
        _builder = new UserBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Act
        var result = _builder.Build();

        // Assert
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldNotBeNull();
        result.Value.UserName.ShouldBe("testuser");
        result.Value.Email.Value.ShouldBe("test@example.com");
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
        result.Errors.ShouldContain(e => e.Code == "ERR_ID_EMPTY");
    }

    [Fact]
    public void Create_ShouldFail_WhenUserNameIsEmpty()
    {
        // Act
        var result = _builder
            .WithUserName(string.Empty)
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_USERNAME_EMPTY");
    }

    [Fact]
    public void Create_ShouldFail_WhenUserNameIsTooLong()
    {
        // Act
        var result = _builder
            .WithUserName(new string('a', 121))
            .Build();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_USERNAME_TOO_LONG");
    }

    [Fact]
    public void UpdateProfile_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var user = _builder.Build().Value;
        var newUsername = "newusername";

        // Act
        var result = user!.UpdateProfile(newUsername: newUsername);

        // Assert
        result.IsFailure.ShouldBeFalse();
        user.UserName.ShouldBe(newUsername);
    }

    [Fact]
    public void UpdateProfile_ShouldFail_WhenUsernameIsEmpty()
    {
        // Arrange
        var user = _builder.Build().Value;

        // Act
        var result = user!.UpdateProfile(newUsername: string.Empty);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Code == "ERR_USERNAME_EMPTY");
    }
}