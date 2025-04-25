using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IPaymentMethodFactory
{
    IPaymentMethod GetPaymentMethod(PaymentMethodEnumDTO paymentMethod);
}