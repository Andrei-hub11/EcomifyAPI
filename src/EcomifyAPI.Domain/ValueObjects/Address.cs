using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct Address
{
    public string Street { get; init; } = string.Empty;
    public int Number { get; init; } = 0;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string Complement { get; init; } = string.Empty;

    public Address(string street, int number, string city, string state, string zipCode, string country, string complement)
    {
        var errors = ValidateAddress(street, number, city, state, zipCode, country, complement);

        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }

        Street = street;
        Number = number;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        Complement = complement;
    }

    private static ReadOnlyCollection<ValidationError> ValidateAddress(string street, int number, string city, string state, string zipCode, string country, string complement)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(street))
        {
            errors.Add(ValidationError.Create("Street is required", "ERR_STREET_REQUIRED", "Street"));
        }

        if (number <= 0)
        {
            errors.Add(ValidationError.Create("Number must be greater than 0", "ERR_NUMBER_GT_0", "Number"));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            errors.Add(ValidationError.Create("City is required", "ERR_CITY_REQUIRED", "City"));
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            errors.Add(ValidationError.Create("State is required", "ERR_STATE_REQUIRED", "State"));
        }

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            errors.Add(ValidationError.Create("ZipCode is required", "ERR_ZIP_CODE_REQUIRED", "ZipCode"));
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            errors.Add(ValidationError.Create("Country is required", "ERR_COUNTRY_REQUIRED", "Country"));
        }

        if (complement is not null && string.IsNullOrWhiteSpace(complement))
        {
            errors.Add(ValidationError.Create("Complement is required", "ERR_COMPLEMENT_REQUIRED", "Complement"));
        }

        return errors.AsReadOnly();
    }
}