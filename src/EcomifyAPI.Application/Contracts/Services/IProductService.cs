using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IProductService
{
    Task<Result<ProductResponseDTO>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> CreateProductAsync(CreateProductRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateProductAsync(UpdateProductRequestDTO request, CancellationToken cancellationToken = default);
}