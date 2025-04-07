namespace EcomifyAPI.Application.Contracts.Services;

public interface IPaymentMethodFactory
{
    IPaymentMethod GetPaymentMethod(string paymentMethod);
}