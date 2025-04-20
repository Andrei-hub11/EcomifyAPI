using EcomifyAPI.Application.Contracts.Discounts;
using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Application.Discounts;

public class DiscountStrategyFactory : IDiscountServiceFactory
{
    private readonly Dictionary<DiscountTypeEnum, IDiscountStrategyResolver> _discountServices;

    public DiscountStrategyFactory(IEnumerable<IDiscountStrategyResolver> discountServices)
    {
        _discountServices = discountServices.ToDictionary(s => s.DiscountType);
    }

    public IDiscountStrategyResolver GetDiscountService(DiscountTypeEnum discountType)
    {
        if (!_discountServices.TryGetValue(discountType, out var discountService))
        {
            throw new ArgumentException($"No discount service found for discount type: {discountType}");
        }

        return discountService;
    }
}