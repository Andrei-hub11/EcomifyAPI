using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Utils.Errors;

public static class DiscountErrorFactory
{
    /// <summary>
    /// Creates an error indicating that a discount was not found.
    /// </summary>
    /// <param name="discountId">The ID of the discount that was not found.</param>
    /// <returns>An <see cref="Error"/> instance representing a discount not found error.</returns>
    public static Error DiscountNotFoundById(Guid discountId) => Error.NotFound($"Discount with id = '{discountId}' was not found.", "ERR_DISCOUNT_NOT_FOUND");

    /// <summary>
    /// Creates an error indicating that a discount was not found.
    /// </summary>
    /// <param name="couponCode">The code of the discount that was not found.</param>
    /// <returns>An <see cref="Error"/> instance representing a discount not found error.</returns>
    public static Error DiscountNotFoundByCode(string couponCode) => Error.NotFound($"Discount with CouponCode = '{couponCode}' was not found.", "ERR_DISCOUNT_NOT_FOUND");

    /// <summary>
    /// Creates an error indicating that a coupon is not valid for use.
    /// </summary>
    /// <param name="discountId">The ID of the discount that is not valid for use.</param>
    /// <returns>An <see cref="Error"/> instance representing a discount not valid for use error.</returns>
    public static Error DiscountNotValidForUse(Guid discountId) => Error.Failure($"Discount with id = '{discountId}' is not valid for use.", "ERR_DISCOUNT_INVALID");

    /// <summary>
    /// Creates an error indicating that a discount is expired.
    /// </summary>
    /// <param name="discountId">The ID of the discount that is expired.</param>
    /// <returns>An <see cref="Error"/> instance representing a discount expired error.</returns>
    public static Error DiscountExpired(Guid discountId) => Error.Failure($"Discount with id = '{discountId}' is expired.", "ERR_DISCOUNT_EXPIRED");

    /// <summary>
    /// Creates an error indicating that the minimum order amount was not reached.
    /// </summary>
    /// <param name="minOrderAmount">The minimum order amount that was not reached.</param>
    /// <returns>An <see cref="Error"/> instance representing a minimum order amount not reached error.</returns>
    public static Error MinimumOrderAmountNotReached(decimal minOrderAmount) => Error.Failure($"The minimum order amount of {minOrderAmount} was not reached.", "ERR_MINIMUM_ORDER_AMOUNT_NOT_REACHED");
}