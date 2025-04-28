using System.Runtime.CompilerServices;

using EcomifyAPI.Application.Contracts.Repositories;

namespace EcomifyAPI.Application.Contracts.Data;

public interface IUnitOfWork : IDisposable
{
    TRepository GetRepository<TRepository>() where TRepository : class, IRepository;
    /// <summary>
    /// Commit the changes to the database asynchronously
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <param name="allowExternalCommit">If true, the transaction will be committed even if it is controlled externally</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task CommitAsync(CancellationToken cancellationToken = default, bool allowExternalCommit = false,
    [CallerMemberName] string caller = "");
    /// <summary>
    /// Rollback the changes to the database asynchronously
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    Task RollbackAsync();
}