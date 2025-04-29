using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
namespace EcomifyAPI.Application.Contracts.Services;

public interface IPaymentService
{
    Task<Result<PaginatedResponseDTO<PaymentResponseDTO>>> GetAsync(PaymentFilterRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponseDTO>> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PaymentResponseDTO>>> GetPaymentsByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
    Task<Result<PaymentResponseDTO>> ProcessPaymentAsync(PaymentRequestDTO request, CancellationToken cancellationToken = default);
    Task<Result<bool>> RefundPaymentAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<Result<bool>> CancelPaymentAsync(Guid transactionId, CancellationToken cancellationToken = default);
}