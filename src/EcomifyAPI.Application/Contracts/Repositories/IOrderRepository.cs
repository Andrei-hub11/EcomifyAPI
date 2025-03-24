using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IOrderRepository : IRepository
{
    Task<OrderMapping?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateOrderAsync(Order order, string currencyCode, CancellationToken cancellationToken = default);
    Task<bool> CreateOrderItemAsync(OrderItem orderItem, Guid orderId, CancellationToken cancellationToken = default);
}