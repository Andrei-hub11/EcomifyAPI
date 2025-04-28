using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Common.Utils.Result;

using Microsoft.Extensions.Logging;

namespace EcomifyAPI.Infrastructure.Data;

public class TransactionHandler : ITransactionHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionHandler> _logger;

    public TransactionHandler(IUnitOfWork unitOfWork, ILogger<TransactionHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Executes a business operation within a transaction and ensures that the transaction is committed only if the operation is successful.
    /// Otherwise, the transaction is rolled back.
    /// </summary>
    /// <typeparam name="T">The type of the result returned by the operation.</typeparam>
    /// <param name="operation">The business operation to execute.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    public async Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> operation, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_unitOfWork is UnitOfWork unitOfWork)
            {
                unitOfWork.SetTransactionControlledExternally(true);
            }
            else
            {
                throw new InvalidOperationException("The unit of work is not a UnitOfWork");
            }

            // Execute the business logic
            var result = await operation();

            // Commit the transaction only if the operation was successful
            if (!result.IsFailure)
            {
                await _unitOfWork.CommitAsync(cancellationToken, true);
                _logger.LogInformation("The transaction was committed successfully");
            }
            else
            {
                // Rollback in case of business error
                await _unitOfWork.RollbackAsync();
                _logger.LogWarning("The transaction was rolled back due to business rules: {Errors}",
                    string.Join(", ", result.Errors));
            }

            return result;
        }
        catch (Exception ex)
        {
            // Rollback in case of unhandled exception
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error during the transaction execution");

            throw;
        }
        finally
        {
            if (_unitOfWork is UnitOfWork unitOfWork)
            {
                unitOfWork.SetTransactionControlledExternally(false);
            }
        }
    }
}