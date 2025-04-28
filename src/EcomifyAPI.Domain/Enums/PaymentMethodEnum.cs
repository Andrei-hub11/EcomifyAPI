using System.ComponentModel;

namespace EcomifyAPI.Domain.Enums;

public enum PaymentMethodEnum
{
    [Description("Credit Card")]
    CreditCard = 1,
    [Description("PayPal")]
    PayPal = 2,
}