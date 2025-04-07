using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
namespace EcomifyAPI.Application.Contracts.Services;

public interface IPaymentService
{
    Task<Result<IReadOnlyList<PaymentResponseDTO>>> GetAsync(CancellationToken cancellationToken = default);
    Task<Result<PaymentResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponseDTO>> ProcessPaymentAsync(PaymentRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<bool>> RefundPaymentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> CancelPaymentAsync(Guid id, CancellationToken cancellationToken = default);
}