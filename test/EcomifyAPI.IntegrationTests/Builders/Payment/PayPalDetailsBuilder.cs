using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class PayPalDetailsBuilder
{
    private Guid _payerId = Guid.NewGuid();
    private string _payerEmail = "test@example.com";

    public PayPalDetailsBuilder WithPayerId(Guid payerId)
    {
        _payerId = payerId;
        return this;
    }

    public PayPalDetailsBuilder WithPayerEmail(string payerEmail)
    {
        _payerEmail = payerEmail;
        return this;
    }

    public PayPalDetailsDTO Build()
    {
        return new PayPalDetailsDTO(
            _payerId,
            _payerEmail
        );
    }
}