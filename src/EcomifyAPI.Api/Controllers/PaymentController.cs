using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Application.Contracts.Services;

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

    [HttpGet]
    public async Task<IActionResult> GetAllPayments(CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetAsync(cancellationToken);

        return result.Match(
            onSuccess: (payments) => Ok(payments),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}