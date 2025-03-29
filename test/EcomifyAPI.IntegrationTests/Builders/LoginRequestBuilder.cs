using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class LoginRequestBuilder
{
    private string _email = "default@example.com";
    private string _password = "Default123!@#";

    public LoginRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public LoginRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public UserLoginRequestDTO Build()
    {
        return new UserLoginRequestDTO(_email, _password);
    }
}