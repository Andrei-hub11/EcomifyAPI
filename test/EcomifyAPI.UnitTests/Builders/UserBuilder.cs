using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Domain.Entities;
namespace EcomifyAPI.UnitTests.Builders;

public class UserBuilder
{
    private Guid? _id = Guid.NewGuid();
    private string _keycloakId = "kc123";
    private string _userName = "testuser";
    private string _email = "test@example.com";
    private readonly string _profileImagePath = "path/to/image.jpg";
    private HashSet<string> _roles = new() { "User" };

    public UserBuilder WithId(Guid? id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithKeycloakId(string keycloakId)
    {
        _keycloakId = keycloakId;
        return this;
    }

    public UserBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithRoles(HashSet<string> roles)
    {
        _roles = roles;
        return this;
    }

    public Result<User> Build()
    {
        return User.Create(
            _keycloakId,
            _userName,
            _email,
            _profileImagePath,
            _roles,
            _id
            );
    }
}