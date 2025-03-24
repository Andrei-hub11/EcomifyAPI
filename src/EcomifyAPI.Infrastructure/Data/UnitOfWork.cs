using System.Data;

using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Repositories;

using Microsoft.Extensions.DependencyInjection;

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

    public UnitOfWork(DapperContext dapperContext, IServiceProvider serviceProvider)
    {
        _dapperContext = dapperContext;
        _serviceProvider = serviceProvider;
        _connection = _dapperContext.CreateConnection();
        _connection.Open();
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

    public void Commit()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            _transaction?.Rollback();
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

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
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
            await RollbackAsync(cancellationToken);
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

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = _connection.BeginTransaction();

        foreach (var repository in _repositories)
        {
            repository.Initialize(_connection, _transaction);
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is NpgsqlTransaction npgTransaction)
        {
            await npgTransaction.RollbackAsync(cancellationToken);
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