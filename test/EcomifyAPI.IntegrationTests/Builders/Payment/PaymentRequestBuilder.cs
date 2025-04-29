using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class PaymentRequestBuilder
{
    private string _userId = string.Empty;
    private PaymentMethodEnumDTO _paymentMethod = PaymentMethodEnumDTO.CreditCard;
    private CreditCardDetailsDTO? _creditCardDetails = null;
    private PayPalDetailsDTO? _payPalDetails = null;
    private AddressRequestDTO _shippingAddress = new(
        "Shipping Street",
        123,
        "São Paulo",
        "SP",
        "01234-567",
        "Brazil",
        "Apartment 123");
    private AddressRequestDTO _billingAddress = new(
        "Billing Street",
        456,
        "São Paulo",
        "SP",
        "01234-567",
        "Brazil",
        "Apartment 456");

    public PaymentRequestBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public PaymentRequestBuilder WithPaymentMethod(PaymentMethodEnumDTO paymentMethod)
    {
        _paymentMethod = paymentMethod;
        return this;
    }

    public PaymentRequestBuilder WithCreditCardDetails(CreditCardDetailsDTO creditCardDetails)
    {
        _paymentMethod = PaymentMethodEnumDTO.CreditCard;
        _creditCardDetails = creditCardDetails;
        _payPalDetails = null;
        return this;
    }

    public PaymentRequestBuilder WithPayPalDetails(PayPalDetailsDTO payPalDetails)
    {
        _paymentMethod = PaymentMethodEnumDTO.PayPal;
        _payPalDetails = payPalDetails;
        _creditCardDetails = null;
        return this;
    }

    public PaymentRequestBuilder WithShippingAddress(AddressRequestDTO shippingAddress)
    {
        _shippingAddress = shippingAddress;
        return this;
    }

    public PaymentRequestBuilder WithBillingAddress(AddressRequestDTO billingAddress)
    {
        _billingAddress = billingAddress;
        return this;
    }

    public PaymentRequestDTO Build()
    {
        return new PaymentRequestDTO(
            _userId,
            _paymentMethod,
            _creditCardDetails,
            _payPalDetails,
            _shippingAddress,
            _billingAddress
        );
    }
}