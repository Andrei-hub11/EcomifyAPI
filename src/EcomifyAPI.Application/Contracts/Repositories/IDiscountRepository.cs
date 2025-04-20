using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IDiscountRepository : IRepository
{
    Task<FilteredResponseMapping<DiscountMapping>> GetAllDiscountsAsync(DiscountFilterRequestDTO filter, CancellationToken cancellationToken = default);
    Task<DiscountMapping?> GetDiscountByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DiscountMapping?> GetDiscountByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<DiscountMapping?> GetDiscountForCartAsync(Guid cartId, CancellationToken cancellationToken = default);
    Task<int> GetUserUsagesAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DiscountHistoryMapping>> GetDiscountHistoryByOrderIdAsync(Guid orderId,
    CancellationToken cancellationToken = default);
    Task<IEnumerable<DiscountMapping>> GetRecentDiscountsByCustomerIdAsync(string customerId, DateTime startDate,
    CancellationToken cancellationToken = default);
    Task<IEnumerable<DiscountMapping>> GetDiscountToApply(Guid cartId, decimal cartTotalAmount,
    CancellationToken cancellationToken = default);
    Task<Guid> CreateDiscountAsync(Discount coupon, CancellationToken cancellationToken = default);
    Task CreateDiscountHistoryAsync(DiscountHistory discountHistory, CancellationToken cancellationToken = default);
    Task LinkDiscountToCategoryAsync(Guid discountId, Guid categoryId, CancellationToken cancellationToken = default);
    Task ApplyDiscountToCartAsync(Guid cartId, Guid discountId, CancellationToken cancellationToken = default);
    Task UpdateDiscountAsync(Discount coupon, CancellationToken cancellationToken = default);
    Task DeleteDiscountAsync(Guid id, CancellationToken cancellationToken = default);
}