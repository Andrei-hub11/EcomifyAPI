using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IShippingService
{
    Task<Result<FreightEstimateResponseDTO>> EstimateShippingAsync(
        EstimateShippingRequestDTO request,
        CancellationToken cancellationToken = default
    );
}