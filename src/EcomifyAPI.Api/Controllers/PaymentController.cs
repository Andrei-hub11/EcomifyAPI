using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[Route("api/v1/payments")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Gets all payments
    /// </summary>
    /// <param name="request">The payment filter request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="PaginatedResponseDTO{PaymentResponseDTO}"/> representing the payments</returns>
    /// <response code="200">The payments were retrieved successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized to access the resource</response>
    [Authorize(Policy = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetPayments([FromQuery] PaymentFilterRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (payments) => Ok(payments),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Gets a payment by transaction id
    /// </summary>
    /// <param name="transactionId">The transaction id of the payment to get</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="PaymentResponseDTO"/> representing the payment</returns>
    /// <response code="200">The payment was retrieved successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized to access the resource</response>
    /// <response code="404">If the payment is not found</response>
    [Authorize(Policy = "Admin")]
    [HttpGet("{transactionId}/details")]
    public async Task<IActionResult> GetPaymentByTransactionId(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetByTransactionIdAsync(transactionId, cancellationToken);

        return result.Match(
            onSuccess: (payment) => Ok(payment),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Gets payments by customer id
    /// </summary>
    /// <param name="id">The id of the customer to get</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="PaymentResponseDTO"/> representing the payments</returns>
    /// <response code="200">The payments were retrieved successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized to access the resource</response>
    [Authorize]
    [HttpGet("{id}/user")]
    public async Task<IActionResult> GetPaymentsByCustomerId(string id, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetPaymentsByCustomerIdAsync(id, cancellationToken);

        return result.Match(
            onSuccess: (payments) => Ok(payments),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }


    /// <summary>
    /// Processes a payment
    /// </summary>
    /// <param name="id">The id of the payment to process</param>
    /// <param name="request">The payment request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="PaymentResponseDTO"/> representing the processed payment</returns>
    /// <response code="200">The payment was processed successfully</response>
    /// <response code="400">The payment request is invalid</response>
    /// <response code="401">The user is not authenticated</response>
    /// <response code="403">The user is not authorized to access the resource</response>
    /// <response code="404">The cart was not found</response>
    /// <response code="422">The payment request is unprocessable</response>
    /// <response code="422">The order request is unprocessable</response>
    [Authorize]
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment(PaymentRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.ProcessPaymentAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (payment) => Ok(payment),
            onFailure: (errors) => errors.ToProblemDetailsResult()
            );
    }

    /// <summary>
    /// Refunds a payment
    /// </summary>
    /// <param name="transactionId">The transaction id of the payment to refund</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="PaymentResponseDTO"/> representing the refunded payment</returns>
    /// <response code="200">The payment was refunded successfully</response>
    /// <response code="400">The payment request is invalid</response>
    /// <response code="401">The user is not authenticated</response>
    /// <response code="403">The user is not authorized to access the resource</response>
    /// <response code="404">The payment is not found</response>
    [Authorize(Policy = "Admin")]
    [HttpPost("{transactionId}/refund")]
    public async Task<IActionResult> RefundPayment(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.RefundPaymentAsync(transactionId, cancellationToken);

        return result.Match(
            onSuccess: (payment) => Ok(payment),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Cancels a payment
    /// </summary>
    /// <param name="transactionId">The transaction id of the payment to cancel</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="PaymentResponseDTO"/> representing the cancelled payment</returns>
    /// <response code="200">The payment was cancelled successfully</response>
    /// <response code="400">The payment request is invalid</response>
    /// <response code="401">The user is not authenticated</response>
    /// <response code="403">The user is not authorized to access the resource</response>
    /// <response code="404">The payment is not found</response>
    [Authorize(Policy = "Admin")]
    [HttpPost("{transactionId}/cancel")]
    public async Task<IActionResult> CancelPayment(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.CancelPaymentAsync(transactionId, cancellationToken);

        return result.Match(
            onSuccess: (payment) => Ok(payment),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}