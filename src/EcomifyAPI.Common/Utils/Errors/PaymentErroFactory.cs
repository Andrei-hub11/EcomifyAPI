using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Utils.Errors;

public static class PaymentErrorFactory
{
    /// <summary>
    /// Creates an error indicating that a payment method is invalid.
    /// </summary>
    /// <param name="method">The invalid payment method.</param>
    /// <returns>An <see cref="Error"/> instance representing an invalid payment method error.</returns>
    public static Error InvalidPaymentMethod(string method) => Error.Failure($"Invalid payment method: '{method}'.", "ERR_PAYMENT_METHOD");

    /// <summary>
    /// Creates an error indicating that a payment failed.
    /// </summary>
    /// <returns>An <see cref="Error"/> instance representing a payment failed error.</returns>
    public static Error PaymentFailed() => Error.Failure("Payment failed. Please check your payment details and try again.", "ERR_PAYMENT_FAILED");

    /// <summary>
    /// Creates an error indicating that a payment has already been processed (refunded/cancelled).
    /// </summary>
    /// <param name="id">The ID of the payment that was already processed.</param>
    /// <returns>An <see cref="Error"/> instance representing a payment already processed error.</returns>
    public static Error PaymentAlreadyProcessed(Guid id) => Error.Failure($"Payment with id = '{id}' has already been refunded or cancelled.", "ERR_PAYMENT_ALREADY_PROCESSED");

    /// <summary>
    /// Creates an error indicating that a payment by transaction id was not found.
    /// </summary>
    /// <param name="transactionId">The transaction id of the payment that was not found.</param>
    /// <returns>An <see cref="Error"/> instance representing a payment not found error.</returns>
    public static Error PaymentNotFoundByTransactionId(Guid transactionId)
    => Error.NotFound($"Payment with transaction id = '{transactionId}' was not found.", "ERR_PAYMENT_NOT_FOUND");

    /// <summary>
    /// Creates an error indicating that a payment does not belong to an order.
    /// </summary>
    /// <param name="paymentId">The ID of the payment that does not belong to an order.</param>
    /// <param name="orderId">The ID of the order that the payment does not belong to.</param>
    /// <returns>An <see cref="Error"/> instance representing a payment does not belong to an order error.</returns>
    public static Error PaymentNotBelongsToOrder(Guid paymentId, Guid orderId)
    => Error.Conflict($"Payment with id = '{paymentId}' does not belong to order with id = '{orderId}'.", "ERR_PAYMENT_ORDER_MISMATCH");

    /// <summary>
    /// Creates an error indicating that a payment does not belong to a user.
    /// </summary>
    /// <param name="paymentId">The ID of the payment that does not belong to a user.</param>
    /// <param name="userId">The ID of the user that the payment does not belong to.</param>
    /// <returns>An <see cref="Error"/> instance representing a payment does not belong to a user error.</returns>
    public static Error PaymentNotBelongsToUser(string userId)
    => Error.Conflict($"Payment does not belong to user with id = '{userId}'.", "ERR_PAYMENT_USER_MISMATCH");
}