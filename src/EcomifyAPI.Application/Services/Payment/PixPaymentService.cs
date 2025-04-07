using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Common.Utils;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Services.Payment;

public class PixPaymentService : IPaymentMethod
{
    public async Task<Result<GatewayResponseDTO>> ProcessPaymentAsync(PaymentDetails request, CancellationToken cancellationToken = default)
    {
        return Result.Ok(new GatewayResponseDTO(
            Guid.NewGuid(),
            OrderIdGenerator.GenerateOrderId(),
            true
        ));
    }
}