using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Application.Contracts.Discounts;

public interface IDiscountServiceFactory
{
    IDiscountStrategyResolver GetDiscountService(DiscountTypeEnum discountType);
}