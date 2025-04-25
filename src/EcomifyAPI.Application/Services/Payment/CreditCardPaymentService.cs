using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Common.Utils;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Validation;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Services.Payment;

public class CreditCardPaymentService : IPaymentMethod
{
    public PaymentMethodEnumDTO PaymentMethod => PaymentMethodEnumDTO.CreditCard;

    public async Task<Result<GatewayResponseDTO>> ProcessPaymentAsync(PaymentDetails request, CancellationToken cancellationToken = default)
    {
        if (request is not CreditCardDetailsDTO creditCardDetails)
        {
            return Result.Fail("Invalid payment details");
        }

        var validationErrors = PaymentValidation.ValidateCreditCard(
            creditCardDetails.CardNumber,
            creditCardDetails.ExpiryDate,
            creditCardDetails.Cvv
        );

        if (validationErrors.Any())
        {
            return Result.Fail(validationErrors);
        }

        await Task.Delay(3000, cancellationToken);

        return Result.Ok(new GatewayResponseDTO(
            Guid.NewGuid(),
            OrderIdGenerator.GenerateOrderId(),
            true));
    }
}