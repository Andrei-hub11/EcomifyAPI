using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Common.Utils;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Services.Payment;

public class PayPalPaymentService : IPaymentMethod
{
    public async Task<Result<GatewayResponseDTO>> ProcessPaymentAsync(PaymentDetails request, CancellationToken cancellationToken = default)
    {
        if (request is not PayPalDetailsDTO payPalDetails)
        {
            return Result.Fail("Invalid payment details");
        }

        await Task.Delay(15000, cancellationToken);

        return Result.Ok(new GatewayResponseDTO(
            Guid.NewGuid(),
            OrderIdGenerator.GenerateOrderId(),
            true
        ));
    }
}