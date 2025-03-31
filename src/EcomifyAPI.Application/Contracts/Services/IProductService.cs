using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IProductService
{
    Task<Result<IReadOnlyList<ProductResponseDTO>>> GetAsync(CancellationToken cancellationToken = default);
    Task<Result<ProductResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> CreateAsync(CreateProductRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateAsync(UpdateProductRequestDTO request, CancellationToken cancellationToken = default);
}