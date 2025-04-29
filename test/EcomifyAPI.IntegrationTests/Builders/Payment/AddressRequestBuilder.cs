using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class AddressRequestBuilder
{
    private string _street = "Test Street";
    private int _number = 123;
    private string _city = "São Paulo";
    private string _state = "SP";
    private string _zipCode = "01234-567";
    private string _country = "Brazil";
    private string _complement = "Apartment 123";

    public AddressRequestBuilder WithStreet(string street)
    {
        _street = street;
        return this;
    }

    public AddressRequestBuilder WithNumber(int number)
    {
        _number = number;
        return this;
    }

    public AddressRequestBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public AddressRequestBuilder WithState(string state)
    {
        _state = state;
        return this;
    }

    public AddressRequestBuilder WithZipCode(string zipCode)
    {
        _zipCode = zipCode;
        return this;
    }

    public AddressRequestBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    public AddressRequestBuilder WithComplement(string complement)
    {
        _complement = complement;
        return this;
    }

    public AddressRequestDTO Build()
    {
        return new AddressRequestDTO(
            _street,
            _number,
            _city,
            _state,
            _zipCode,
            _country,
            _complement
        );
    }
}