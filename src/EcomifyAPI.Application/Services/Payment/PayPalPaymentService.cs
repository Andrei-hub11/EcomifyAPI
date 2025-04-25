using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Common.Utils;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Validation;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Services.Payment;

public class PayPalPaymentService : IPaymentMethod
{
    public PaymentMethodEnumDTO PaymentMethod => PaymentMethodEnumDTO.PayPal;

    public async Task<Result<GatewayResponseDTO>> ProcessPaymentAsync(PaymentDetails request, CancellationToken cancellationToken = default)
    {
        if (request is not PayPalDetailsDTO payPalDetails)
        {
            return Result.Fail("Invalid payment details");
        }

        var validationErrors = PaymentValidation.ValidatePayPal(
            payPalDetails.PayerEmail,
            payPalDetails.PayerId
        );

        if (validationErrors.Any())
        {
            return Result.Fail(validationErrors);
        }

        await Task.Delay(100, cancellationToken);

        return Result.Ok(new GatewayResponseDTO(
            Guid.NewGuid(),
            OrderIdGenerator.GenerateOrderId(),
            true
        ));
    }
}