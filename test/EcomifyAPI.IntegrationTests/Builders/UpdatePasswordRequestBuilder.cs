using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class UpdatePasswordRequestBuilder
{
    private string _userId = string.Empty;
    private string _newPassword = string.Empty;
    private string _token = string.Empty;

    public UpdatePasswordRequestBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public UpdatePasswordRequestBuilder WithNewPassword(string newPassword)
    {
        _newPassword = newPassword;
        return this;
    }

    public UpdatePasswordRequestBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public UpdatePasswordRequestDTO Build()
    {
        return new UpdatePasswordRequestDTO(_userId, _newPassword, _token);
    }
}