using EcomifyAPI.Common.Utils.Result;

namespace EcomifyAPI.Application.Contracts.Data;

public interface ITransactionHandler
{
    Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> operation, CancellationToken cancellationToken = default);
}