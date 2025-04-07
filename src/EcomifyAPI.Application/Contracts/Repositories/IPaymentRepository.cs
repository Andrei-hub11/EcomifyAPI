using EcomifyAPI.Contracts.DapperModels;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IPaymentRepository : IRepository
{
    Task<IEnumerable<PaymentRecordMapping>> GetAsync(CancellationToken cancellationToken = default);
    Task<PaymentRecordMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default);
}