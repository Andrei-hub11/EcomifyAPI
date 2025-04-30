using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IOrderService
{
    Task<Result<IReadOnlyList<OrderResponseDTO>>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<OrderResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaginatedResponseDTO<OrderResponseDTO>>> GetFilteredAsync(OrderFilterRequestDTO filter, CancellationToken cancellationToken = default);
    Task<Result<OrderResponseDTO>> CreateOrderAsync(CreateOrderRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateStatusAsync(Guid orderId, OrderStatusEnum status, CancellationToken cancellationToken = default);
    Task<Result<bool>> MarkAsShippedAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<bool>> MarkAsCompletedAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default);
}