using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Authorization;
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
    /// Retrieves all orders for a specific user.
    /// </summary>
    /// <param name="userId">The user ID of the orders to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A list of <see cref="OrderResponseDTO"/> containing the order details.
    /// </returns>
    /// <response code="200">Returns the list of orders.</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="403">Returned when the user is not authorized to access the resource.</response>
    [Authorize]
    [HttpGet("{userId}/user")]
    public async Task<IActionResult> GetOrders(string userId, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetByUserIdAsync(userId, cancellationToken);

        return result.Match(
            onSuccess: (orders) => Ok(orders),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Retrieves filtered orders with pagination for admin users.
    /// </summary>
    /// <param name="filter">The filter parameters to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A paginated list of <see cref="OrderResponseDTO"/> containing the filtered order details.
    /// </returns>
    /// <response code="200">Returns the paginated list of filtered orders.</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="403">Returned when the user is not authorized to access the resource.</response>
    [Authorize(Roles = "Admin")]
    [HttpGet("filter")]
    public async Task<IActionResult> GetFilteredOrders([FromQuery] OrderFilterRequestDTO filter, CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetFilteredAsync(filter, cancellationToken);

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
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="403">Returned when the user is not authorized to access the resource.</response>
    /// <response code="404">Returned when no order with the specified ID exists.</response>
    [Authorize(Roles = "Admin")]
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
    /// Updates the status of an order.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>
    /// A boolean value indicating whether the order was updated successfully.
    /// </returns>
    /// <response code="200">Returns true if the order was updated successfully.</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="403">Returned when the user is not authorized to access the resource.</response>
    /// <response code="404">Returned when no order with the specified ID exists.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/completed")]
    public async Task<IActionResult> MarkAsCompleted(Guid id)
    {
        var result = await _orderService.MarkAsCompletedAsync(id);

        return result.Match(
            onSuccess: (isUpdated) => Ok(isUpdated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Marks an order as completed.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>
    /// A boolean value indicating whether the order was updated successfully.
    /// </returns>
    /// <response code="200">Returns true if the order was updated successfully.</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="403">Returned when the user is not authorized to access the resource.</response>
    /// <response code="404">Returned when no order with the specified ID exists.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/shipped")]
    public async Task<IActionResult> MarkAsShipped(Guid id)
    {
        var result = await _orderService.MarkAsShippedAsync(id);

        return result.Match(
            onSuccess: (isUpdated) => Ok(isUpdated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /*     /// <summary>
        /// Deletes an order by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the order.</param>
        /// <returns>
        /// A boolean value indicating whether the order was deleted successfully.
        /// </returns>
        /// <response code="200">Returns true if the order was deleted successfully.</response>
        /// <response code="401">Returned when the user is not authenticated.</response>
        /// <response code="403">Returned when the user is not authorized to access the resource.</response>
        /// <response code="404">Returned when no order with the specified ID exists.</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var result = await _orderService.DeleteOrderAsync(id);

            return result.Match(
                onSuccess: (isDeleted) => Ok(isDeleted),
                onFailure: (errors) => errors.ToProblemDetailsResult()
            );
        } */
}