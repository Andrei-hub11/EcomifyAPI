using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class UpdateUserRequestBuilder
{
    private string _userName = "default";
    /*  private string _email = "default@test.com"; */
    private string _password = "Default123!@#";
    private string _profileImage = "";

    public UpdateUserRequestBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }

    /*     public UpdateUserRequestBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        } */

    public UpdateUserRequestBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public UpdateUserRequestBuilder WithProfileImage(string profileImage)
    {
        _profileImage = profileImage;
        return this;
    }

    public UpdateUserRequestDTO Build()
    {
        return new UpdateUserRequestDTO(_userName, _password, _profileImage);
    }
}