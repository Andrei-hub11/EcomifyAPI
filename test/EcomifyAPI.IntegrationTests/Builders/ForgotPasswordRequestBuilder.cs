using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class ForgotPasswordRequestBuilder
{
    private string _email = "default@test.com";

    public ForgotPasswordRequestBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public ForgetPasswordRequestDTO Build()
    {
        return new ForgetPasswordRequestDTO(_email);
    }
}