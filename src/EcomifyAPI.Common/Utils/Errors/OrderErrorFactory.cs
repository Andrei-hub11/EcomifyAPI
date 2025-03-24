using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Utils.Errors;

public static class OrderErrorFactory
{

    /// <summary>
    /// Creates a not found error for a order by ID.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <returns>An <see cref="Error"/> instance representing a order not found error.</returns>
    public static Error OrderNotFound(Guid id)
    {
        return Error.NotFound($"Order with id = '{id}' was not found.", "ERR_ORDER_NOT_FOUND");
    }
}