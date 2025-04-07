using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IOrderRepository : IRepository
{
    Task<IEnumerable<OrderMapping>> GetAsync(CancellationToken cancellationToken = default);
    Task<OrderMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Order order, string currencyCode, CancellationToken cancellationToken = default);
    Task<bool> CreateOrderItemAsync(OrderItem orderItem, Guid orderId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid orderId, CancellationToken cancellationToken = default);
}