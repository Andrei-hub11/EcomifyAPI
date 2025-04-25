using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Application.Services.Payment;

public class PaymentMethodFactory : IPaymentMethodFactory
{
    private readonly Dictionary<PaymentMethodEnumDTO, IPaymentMethod> _paymentMethods;

    public PaymentMethodFactory(IEnumerable<IPaymentMethod> paymentMethods)
    {
        _paymentMethods = paymentMethods.ToDictionary(pm => pm.PaymentMethod, pm => pm);
    }

    public IPaymentMethod GetPaymentMethod(PaymentMethodEnumDTO paymentMethod)
    {
        if (_paymentMethods.TryGetValue(paymentMethod, out var method))
            return method;

        throw new ArgumentException("Invalid payment method");
    }
}