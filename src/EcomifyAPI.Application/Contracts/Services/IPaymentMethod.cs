using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IPaymentMethod
{
    Task<Result<GatewayResponseDTO>> ProcessPaymentAsync(PaymentDetails request, CancellationToken cancellationToken = default);
}