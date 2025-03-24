using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Utils.Errors;

public static class ProductErrorFactory
{
    /// <summary>
    /// Creates an error indicating that a product is out of stock.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>An <see cref="Error"/> instance representing a product out of stock error.</returns>
    public static Error ProductOutOfStock(Guid id)
    {
        return Error.Failure($"Product with id = '{id}' is out of stock.", "ERR_PRODUCT_OUT_OF_STOCK");
    }

    /// <summary>
    /// Creates a not found error for a product by ID.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>An <see cref="Error"/> instance representing a product not found error.</returns>
    public static Error ProductNotFoundById(Guid id)
    {
        return Error.NotFound($"Product with id = '{id}' was not found.", "ERR_PRODUCT_NOT_FOUND");
    }

    /// <summary>
    /// Creates a not found error for a category by ID.
    /// </summary>
    /// <param name="id">The category identifier.</param>
    /// <returns>An <see cref="Error"/> instance representing a category not found error.</returns>
    public static Error CategoryNotFoundById(Guid id)
    {
        return Error.NotFound($"Category with id = '{id}' was not found.", "ERR_CATEGORY_NOT_FOUND");
    }


}