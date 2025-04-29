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

    /// <summary>
    /// Estimates the shipping cost for an order
    /// </summary>
    /// <param name="request">The request containing the shipping details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The <see cref="ShippingResponseDTO"/> representing the estimated shipping cost</returns>
    /// <response code="200">The shipping cost was estimated successfully</response>
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