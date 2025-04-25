using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class PaymentRecordBuilder
{
    private Guid _paymentId = Guid.NewGuid();
    private Guid _orderId = Guid.NewGuid();
    private Money _amount = new("USD", 100m);
    private PaymentMethodEnum _paymentMethod = PaymentMethodEnum.CreditCard;
    private Guid _transactionId = Guid.NewGuid();
    private string _gatewayResponse = "Success";
    private string _lastFourDigits = "4242";
    private string _cardBrand = "Visa";
    private string _paypalEmail = "test@example.com";
    private string _paypalPayerId = "PAYERID123";

    public PaymentRecordBuilder WithPaymentId(Guid paymentId)
    {
        _paymentId = paymentId;
        return this;
    }

    public PaymentRecordBuilder WithOrderId(Guid orderId)
    {
        _orderId = orderId;
        return this;
    }

    public PaymentRecordBuilder WithAmount(Money amount)
    {
        _amount = amount;
        return this;
    }

    public PaymentRecordBuilder WithPaymentMethod(PaymentMethodEnum paymentMethod)
    {
        _paymentMethod = paymentMethod;
        return this;
    }

    public PaymentRecordBuilder WithTransactionId(Guid transactionId)
    {
        _transactionId = transactionId;
        return this;
    }

    public PaymentRecordBuilder WithGatewayResponse(string gatewayResponse)
    {
        _gatewayResponse = gatewayResponse;
        return this;
    }

    public PaymentRecordBuilder WithLastFourDigits(string lastFourDigits)
    {
        _lastFourDigits = lastFourDigits;
        return this;
    }

    public PaymentRecordBuilder WithCardBrand(string cardBrand)
    {
        _cardBrand = cardBrand;
        return this;
    }

    public PaymentRecordBuilder WithPayPalEmail(string paypalEmail)
    {
        _paypalEmail = paypalEmail;
        return this;
    }

    public PaymentRecordBuilder WithPayPalPayerId(string paypalPayerId)
    {
        _paypalPayerId = paypalPayerId;
        return this;
    }

    public PaymentRecord BuildCreditCardPayment()
    {
        return PaymentRecord.CreateCreditCardPayment(
            _orderId,
            _amount,
            _transactionId,
            _lastFourDigits,
            _cardBrand,
            _gatewayResponse);
    }

    public PaymentRecord BuildPayPalPayment()
    {
        return PaymentRecord.CreatePayPalPayment(
            _orderId,
            _amount,
            _transactionId,
            _paypalEmail,
            _paypalPayerId,
            _gatewayResponse);
    }

    public PaymentRecord BuildFrom()
    {
        IPaymentMethodDetails paymentMethodDetails;

        if (_paymentMethod == PaymentMethodEnum.CreditCard)
        {
            paymentMethodDetails = new CreditCardDetails(_lastFourDigits, _cardBrand);
        }
        else
        {
            paymentMethodDetails = new PayPalDetails(_paypalEmail, _paypalPayerId);
        }

        return PaymentRecord.From(
            _paymentId,
            _orderId,
            _amount,
            _paymentMethod,
            _transactionId,
            _gatewayResponse,
            paymentMethodDetails);
    }
}