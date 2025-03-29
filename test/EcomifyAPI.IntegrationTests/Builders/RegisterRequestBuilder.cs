using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class RegisterRequestBuilder
{
    private string _userName = "default";
    private string _email = "default@example.com";
    private string _password = "Default123!@#";
    private string _profileImage = "";

    public RegisterRequestBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    public RegisterRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public RegisterRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public RegisterRequestBuilder WithProfileImage(string profileImage)
    {
        _profileImage = profileImage;
        return this;
    }

    public UserRegisterRequestDTO Build()
    {
        return new UserRegisterRequestDTO(_userName, _email, _password, _profileImage);
    }
}