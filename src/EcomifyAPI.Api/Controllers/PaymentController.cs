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

    [Authorize(Policy = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllPayments(CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetAsync(cancellationToken);

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
}