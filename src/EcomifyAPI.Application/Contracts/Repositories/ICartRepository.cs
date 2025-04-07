using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface ICartRepository : IRepository
{
    Task<CartMapping?> GetCartAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartMapping> CreateCartAsync(Cart cart, CancellationToken cancellationToken = default);
    Task AddItemAsync(Guid cartId, CartItem item, CancellationToken cancellationToken = default);
    Task UpdateItemQuantityAsync(Guid cartId, Guid productId, CartItem item, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(Guid cartId, Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default);
}