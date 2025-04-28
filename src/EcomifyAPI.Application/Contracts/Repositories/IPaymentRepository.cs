using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IPaymentRepository : IRepository
{
    Task<FilteredResponseMapping<PaymentRecordMapping>> GetAsync(PaymentFilterRequestDTO request,
    CancellationToken cancellationToken = default);
    Task<PaymentRecordMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentRecordMapping?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentRecordMapping>> GetPaymentsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default);
    Task CreateStatusHistoryAsync(Guid paymentId, PaymentStatusChange statusChange, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default);
}