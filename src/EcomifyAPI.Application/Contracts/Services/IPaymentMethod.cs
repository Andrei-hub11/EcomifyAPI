using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IPaymentMethod
{
    PaymentMethodEnumDTO PaymentMethod { get; }
    Task<Result<GatewayResponseDTO>> ProcessPaymentAsync(PaymentDetails request, CancellationToken cancellationToken = default);
}