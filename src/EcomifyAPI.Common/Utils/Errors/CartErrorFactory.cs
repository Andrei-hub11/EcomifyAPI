using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Utils.Errors;

public static class CartErrorFactory
{
    /// <summary>
    /// Creates an error indicating that a cart was not found.
    /// </summary>
    /// <param name="userId">The ID of the user that the cart was not found.</param>
    /// <returns>An <see cref="Error"/> instance representing a cart not found error.</returns>
    public static Error CartNotFound(string userId) => Error.NotFound($"Cart of user with id = '{userId}' was not found.", "ERR_CART_NOT_FOUND");


    /// <summary>
    /// Creates an error indicating that a cart item was not found.
    /// </summary>
    /// <param name="productId">The ID of the product that the cart item was not found.</param>
    /// <returns>An <see cref="Error"/> instance representing a cart item not found error.</returns>
    public static Error CartItemNotFound(Guid productId) => Error.NotFound($"Cart item with product id = '{productId}' was not found.", "ERR_CART_ITEM_NOT_FOUND");
}