using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface ICartService
{
    Task<Result<CartResponseDTO>> GetCartAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<CartResponseDTO>> AddItemAsync(string userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<Result<CartResponseDTO>> RemoveItemAsync(string userId, Guid productId, CancellationToken cancellationToken = default);
    Task<Result<CartResponseDTO>> UpdateItemQuantityAsync(string userId, Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<Result<CartResponseDTO>> ClearCartAsync(string userId, CancellationToken cancellationToken = default);
}