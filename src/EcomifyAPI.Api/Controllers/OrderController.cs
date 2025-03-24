using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[ServiceFilter(typeof(ResultFilter))]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Get order by id
    /// </summary>
    /// <param name="id">Order id</param>
    /// <returns><see cref="OrderResponseDTO"/> Order searched</returns>
    /// <response code="200">Order found</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);

        return result.Match(
            onSuccess: (order) => Ok(order),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Create order
    /// </summary>
    /// <param name="request"><see cref="CreateOrderRequestDTO"/> Order request</param>
    /// <returns><see cref="bool"/> Order created</returns>
    /// <response code="200">Order created</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequestDTO request)
    {
        var result = await _orderService.CreateOrderAsync(request);

        return result.Match(
            onSuccess: (isCreated) => Ok(isCreated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}