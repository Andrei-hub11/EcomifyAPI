using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[Route("api/v1/orders")]
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
    /// Retrieves all orders.
    /// </summary>
    /// <returns>
    /// A list of <see cref="OrderResponseDTO"/> containing the order details.
    /// </returns>
    /// <response code="200">Returns the list of orders.</response>
    /// <response code="400">Some invalid data was provided.</response>
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var result = await _orderService.GetAsync();

        return result.Match(
            onSuccess: (orders) => Ok(orders),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>
    /// A <see cref="OrderResponseDTO"/> containing the order details if found.
    /// </returns>
    /// <response code="200">Returns the order details when found successfully.</response>
    /// <response code="404">Returned when no order with the specified ID exists.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var result = await _orderService.GetByIdAsync(id);

        return result.Match(
            onSuccess: (order) => Ok(order),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="request"><see cref="CreateOrderRequestDTO"/> Order request</param>
    /// <returns>
    /// A boolean value indicating whether the order was created successfully.
    /// </returns>
    /// <response code="200">Returns true if the order was created successfully.</response>
    /// <response code="400">Some invalid data was provided.</response>
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequestDTO request)
    {
        var result = await _orderService.CreateOrderAsync(request);

        return result.Match(
            onSuccess: (isCreated) => Ok(isCreated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>
    /// A boolean value indicating whether the order was deleted successfully.
    /// </returns>
    /// <response code="200">Returns true if the order was deleted successfully.</response>
    /// <response code="404">Returned when no order with the specified ID exists.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var result = await _orderService.DeleteOrderAsync(id);

        return result.Match(
            onSuccess: (isDeleted) => Ok(isDeleted),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}