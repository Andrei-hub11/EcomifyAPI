using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class CreditCardDetailsBuilder
{
    private string _cardNumber = "4111111111111111";
    private string _cardholderName = "Test User";
    private string _expiryDate = "12/25";
    private string _cvv = "123";

    public CreditCardDetailsBuilder WithCardNumber(string cardNumber)
    {
        _cardNumber = cardNumber;
        return this;
    }

    public CreditCardDetailsBuilder WithCardholderName(string cardholderName)
    {
        _cardholderName = cardholderName;
        return this;
    }

    public CreditCardDetailsBuilder WithExpiryDate(string expiryDate)
    {
        _expiryDate = expiryDate;
        return this;
    }

    public CreditCardDetailsBuilder WithCvv(string cvv)
    {
        _cvv = cvv;
        return this;
    }

    public CreditCardDetailsDTO Build()
    {
        return new CreditCardDetailsDTO(
            _cardNumber,
            _cardholderName,
            _expiryDate,
            _cvv
        );
    }
}