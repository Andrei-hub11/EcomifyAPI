using EcomifyAPI.Application.Contracts.Repositories;

namespace EcomifyAPI.Application.Contracts.Data;

public interface IUnitOfWork : IDisposable
{
    TRepository GetRepository<TRepository>() where TRepository : class, IRepository;
    void Commit();
    void Rollback();
    /// <summary>
    /// Commit the changes to the database asynchronously
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task CommitAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Rollback the changes to the database asynchronously
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}