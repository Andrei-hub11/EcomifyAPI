using EcomifyAPI.Common.Utils.Errors;

namespace EcomifyAPI.UnitTests.Utillities;

public class UserErrorFactoryTest
{
    [Fact]
    public void UserNotFoundById_ShouldReturnUserNotFoundError()
    {
        // Arrange
        var userId = "123";

        // Act
        var error = UserErrorFactory.UserNotFoundById(userId);

        // Assert
        Assert.Equal("ERR_USER_NOT_FOUND", error.Code);
        Assert.Equal($"User with id = '{userId}' was not found.", error.Description);
    }

    [Fact]
    public void UserNotFoundByName_ShouldReturnUserNotFoundError()
    {
        // Arrange
        var userName = "john_doe";

        // Act
        var error = UserErrorFactory.UserNotFoundByName(userName);

        // Assert
        Assert.Equal("ERR_USER_NOT_FOUND", error.Code);
        Assert.Equal($"User with username = '{userName}' was not found.", error.Description);
    }

    [Fact]
    public void UserNotFoundByEmail_ShouldReturnUserNotFoundError()
    {
        // Arrange
        var email = "john@example.com";

        // Act
        var error = UserErrorFactory.UserNotFoundByEmail(email);

        // Assert
        Assert.Equal("ERR_USER_NOT_FOUND", error.Code);
        Assert.Equal($"User with email = '{email}' was not found.", error.Description);
    }

    [Fact]
    public void UsernameAlreadyExists_ShouldReturnConflictError()
    {
        // Act
        var error = UserErrorFactory.UsernameAlreadyExists();

        // Assert
        Assert.Equal("ERR_DUPLICATE_USERNAME", error.Code);
        Assert.Equal("An account with the provided username is already registered.", error.Description);
    }

    [Fact]
    public void EmailAlreadyExists_ShouldReturnConflictError()
    {
        // Act
        var error = UserErrorFactory.EmailAlreadyExists();

        // Assert
        Assert.Equal("ERR_DUPLICATE_EMAIL", error.Code);
        Assert.Equal($"An account with the provided email is already registered.", error.Description);
    }
}