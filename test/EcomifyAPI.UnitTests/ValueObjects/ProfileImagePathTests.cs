using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.ValueObjects;

public class ProfileImagePathTests
{
    private readonly ProfileImagePathBuilder _builder;

    public ProfileImagePathTests()
    {
        _builder = new ProfileImagePathBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidPathProvided()
    {
        // Arrange
        var path = "path/to/profile/image.jpg";

        // Act
        var result = _builder.WithPath(path).Build();

        // Assert
        result.Value.ShouldBe(path);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_ShouldSucceed_WhenNullOrEmptyPathProvided(string? path)
    {
        var result = _builder.WithPath(path!).Build();

        // Assert
        result.Value.ShouldBe(string.Empty);
    }

    [Fact]
    public void Create_ShouldFail_WhenPathIsTooLong()
    {
        // Arrange
        var path = new string('a', 256);

        // Act
        Should.Throw<DomainException>(() => _builder.WithPath(path).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_IMG_PATH_LONG"
        && e.Description == "Profile image path cannot be longer than 255 characters" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenPathIsRooted()
    {
        // Arrange
        var path = "C:\\path\\to\\profile\\image.jpg";

        // Act
        Should.Throw<DomainException>(() => _builder.WithPath(path).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_IMG_PATH_ROOTED"
        && e.Description == "Profile image path cannot be a rooted path" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenPathContainsInvalidCharacters()
    {
        // Arrange
        var path = "folder/subfolder/invalid<>:\"/\\|?*.txt";

        // Act
        Should.Throw<DomainException>(() => _builder.WithPath(path).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_IMG_PATH_INV_CHAR"
        && e.Description == "Profile image path contains invalid characters" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenPathHasInvalidExtension()
    {
        // Arrange
        var path = "path/to/profile/image.txt";

        // Act
        Should.Throw<DomainException>(() => _builder.WithPath(path).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_IMG_PATH_EXT"
        && e.Description == "Profile image path has an invalid extension" && e.ErrorType == ErrorType.Validation);
    }
}