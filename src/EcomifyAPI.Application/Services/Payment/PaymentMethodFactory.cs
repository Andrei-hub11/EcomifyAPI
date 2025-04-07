using EcomifyAPI.Application.Contracts.Services;

namespace EcomifyAPI.Application.Services.Payment;

public class PaymentMethodFactory : IPaymentMethodFactory
{
    private readonly Dictionary<string, IPaymentMethod> _paymentMethods;

    public PaymentMethodFactory(IEnumerable<IPaymentMethod> paymentMethods)
    {
        _paymentMethods = paymentMethods.ToDictionary(
        pm => pm.GetType().Name.Replace("PaymentMethod", "").ToLower(), pm => pm);
    }

    public IPaymentMethod GetPaymentMethod(string paymentMethod)
    {
        if (_paymentMethods.TryGetValue(paymentMethod.ToLower(), out var method))
            return method;

        throw new ArgumentException("Método de pagamento inválido");
    }
}