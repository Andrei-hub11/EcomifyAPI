using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.Application.Contracts.Discounts;

public interface IDiscountStrategyResolver
{
    DiscountTypeEnum DiscountType { get; }
    Task<Result<decimal>> ApplyDiscountAsync(decimal orderAmount, ApplyDiscountRequestDTO request,
    CancellationToken cancellationToken = default);

    Task<Result<decimal>> CalculateTotalDiscountAsync(decimal cartAmount, HashSet<Guid> discountIds,
    CancellationToken cancellationToken = default);
}