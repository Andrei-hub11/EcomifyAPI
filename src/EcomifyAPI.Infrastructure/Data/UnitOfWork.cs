using System.Data;
using System.Runtime.CompilerServices;

using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Repositories;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace EcomifyAPI.Infrastructure.Data;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DapperContext _dapperContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbConnection _connection;
    private IDbTransaction _transaction;
    private bool _disposed;
    private readonly List<IRepository> _repositories = [];
    private readonly ILogger<UnitOfWork> _logger;
    private bool _isTransactionControlledExternally = false;

    public UnitOfWork(DapperContext dapperContext, IServiceProvider serviceProvider, ILogger<UnitOfWork> logger)
    {
        _dapperContext = dapperContext;
        _serviceProvider = serviceProvider;
        _connection = _dapperContext.CreateConnection();
        _connection.Open();
        _logger = logger;
        _transaction = _connection.BeginTransaction();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is NpgsqlConnection npgConnection)
        {
            await npgConnection.OpenAsync(cancellationToken);
        }
        else
        {
            _connection.Open();
        }
        _transaction = _connection.BeginTransaction();
    }

    public TRepository GetRepository<TRepository>()
        where TRepository : class, IRepository
    {
        var repository = _serviceProvider.GetRequiredService<TRepository>();
        repository.Initialize(_connection, _transaction);
        _repositories.Add(repository);
        return repository;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default, bool allowExternalCommit = false,
    [CallerMemberName] string caller = "")
    {
        if (_isTransactionControlledExternally && !allowExternalCommit)
        {
            _logger.LogInformation("[{caller}] The transaction is controlled externally, skipping commit", caller);
            return;
        }

        try
        {
            if (_transaction is NpgsqlTransaction npgTransaction)
            {
                await npgTransaction.CommitAsync(cancellationToken);
            }
            else
            {
                _transaction?.Commit();
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = _connection.BeginTransaction();

            foreach (var repository in _repositories)
            {
                repository.Initialize(_connection, _transaction);
            }
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction is NpgsqlTransaction npgTransaction)
        {
            await npgTransaction.RollbackAsync();
        }
        else
        {
            _transaction?.Rollback();
        }

        _transaction?.Dispose();
        _transaction = _connection.BeginTransaction();

        foreach (var repository in _repositories)
        {
            repository.Initialize(_connection, _transaction);
        }
    }

    /// <summary>
    /// Defines whether the transaction should be controlled externally. This is critical for ensuring that atomic operations 
    /// across multiple services or repositories are handled correctly. When set to true, the transaction lifecycle (begin, commit, rollback) 
    /// is managed externally by the calling code, allowing finer control over when the transaction is committed or rolled back.
    /// </summary>
    /// <param name="isControlled">A boolean value indicating whether the transaction is controlled externally. 
    /// If <c>true</c>, the transaction will be controlled externally; if <c>false</c>, the transaction will be handled 
    /// internally by the <see cref="UnitOfWork"/>.</param>
    /// <remarks>
    /// This method is typically used in scenarios where multiple operations need to be coordinated within a single transaction,
    /// and the calling code (such as a <see cref="TransactionHandler"/>) needs to determine when to commit or roll back the transaction.
    /// 
    /// Setting the flag to <c>true</c> will prevent internal commits or rollbacks from occurring and delegate transaction control 
    /// to the external code. It is important to call <see cref="SetTransactionControlledExternally"/> with <c>false</c> after 
    /// the external transaction handling is complete to ensure that future operations are handled as part of the normal transaction lifecycle.
    /// </remarks>
    internal void SetTransactionControlledExternally(bool isControlled)
    {
        _isTransactionControlledExternally = isControlled;
    }

    // protected virtual won't be necessary because it's a sealed class
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}