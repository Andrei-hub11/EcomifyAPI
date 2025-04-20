using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[Route("api/v1/shipping")]
[ApiController]
[ServiceFilter(typeof(ResultFilter))]
public class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    [HttpPost("estimate")]
    public async Task<IActionResult> EstimateShipping([FromBody] EstimateShippingRequestDTO request,
    CancellationToken cancellationToken = default)
    {
        var result = await _shippingService.EstimateShippingAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (shipping) => Ok(shipping),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}