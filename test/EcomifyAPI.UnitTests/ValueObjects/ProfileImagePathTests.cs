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

    [Fact]
    public void Create_ShouldFail_WhenPathIsEmpty()
    {
        // Arrange
        var path = "";

        // Act
        Should.Throw<ArgumentException>(() => _builder.WithPath(path).Build())
        .Message.ShouldBe("Profile image path cannot be empty (Parameter 'value')");
    }

    [Fact]
    public void Create_ShouldFail_WhenPathIsTooLong()
    {
        // Arrange
        var path = new string('a', 256);

        // Act
        Should.Throw<ArgumentException>(() => _builder.WithPath(path).Build())
        .Message.ShouldBe("Profile image path cannot be longer than 255 characters (Parameter 'value')");
    }

    [Fact]
    public void Create_ShouldFail_WhenPathIsRooted()
    {
        // Arrange
        var path = "C:\\path\\to\\profile\\image.jpg";

        // Act
        Should.Throw<ArgumentException>(() => _builder.WithPath(path).Build())
        .Message.ShouldBe("Profile image path cannot be a rooted path (Parameter 'value')");
    }

    [Fact]
    public void Create_ShouldFail_WhenPathContainsInvalidCharacters()
    {
        // Arrange
        var path = "folder/subfolder/invalid<>:\"/\\|?*.txt";

        // Act
        Should.Throw<ArgumentException>(() => _builder.WithPath(path).Build())
        .Message.ShouldBe("Profile image path contains invalid characters (Parameter 'value')");
    }

    [Fact]
    public void Create_ShouldFail_WhenPathHasInvalidExtension()
    {
        // Arrange
        var path = "path/to/profile/image.txt";

        // Act
        Should.Throw<ArgumentException>(() => _builder.WithPath(path).Build())
        .Message.ShouldBe("Profile image path has an invalid extension (Parameter 'value')");
    }
}