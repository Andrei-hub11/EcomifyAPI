using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Common;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IDiscountService
{
    Task<Result<PaginatedResponseDTO<DiscountResponseDTO>>> GetAllAsync(DiscountFilterRequestDTO filter, CancellationToken cancellationToken = default);
    Task<Result<DiscountResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DiscountHistoryResponseDTO>>> GetDiscountHistoryByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<DiscountResponseDTO>>> GetDiscountToApply(string userId, CancellationToken cancellationToken = default);
    Task<Result<DiscountResponseDTO>> CreateAsync(CreateDiscountRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<bool>> CreateDiscountHistoryAsync(DiscountHistory discountHistory, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ClearAppliedDiscountsAsync(Guid cartId, CancellationToken cancellationToken = default);
}