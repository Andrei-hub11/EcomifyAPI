using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class UpdateAccessTokenRequestBuilder
{
    private string _refreshToken = string.Empty;

    public UpdateAccessTokenRequestBuilder WithRefreshToken(string refreshToken)
    {
        _refreshToken = refreshToken;
        return this;
    }

    public UpdateAccessTokenRequestDTO Build()
    {
        return new UpdateAccessTokenRequestDTO(_refreshToken);
    }
}