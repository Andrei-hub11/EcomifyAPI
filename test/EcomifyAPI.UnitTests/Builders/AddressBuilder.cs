using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.ValueObjects.Builders;

public class AddressBuilder
{
    private string _street = "123 Main St";
    private int _number = 1;
    private string _complement = "Apt 4B";
    private string _city = "New York";
    private string _state = "NY";
    private string _zipCode = "10001";
    private string _country = "United States";

    public AddressBuilder WithStreet(string street)
    {
        _street = street;
        return this;
    }

    public AddressBuilder WithNumber(int number)
    {
        _number = number;
        return this;
    }

    public AddressBuilder WithComplement(string complement)
    {
        _complement = complement;
        return this;
    }

    public AddressBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public AddressBuilder WithState(string state)
    {
        _state = state;
        return this;
    }

    public AddressBuilder WithZipCode(string zipCode)
    {
        _zipCode = zipCode;
        return this;
    }

    public AddressBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    public Address Build()
    {
        return new Address(_street, _number, _city, _state, _zipCode, _country, _complement);
    }
}