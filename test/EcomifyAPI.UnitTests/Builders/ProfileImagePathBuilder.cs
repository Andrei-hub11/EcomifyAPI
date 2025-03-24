using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class ProfileImagePathBuilder
{
    private string _path = "path/to/profile/image.jpg";

    public ProfileImagePathBuilder WithPath(string path)
    {
        _path = path;
        return this;
    }

    public ProfileImagePath Build()
    {
        return new ProfileImagePath(_path);
    }
}