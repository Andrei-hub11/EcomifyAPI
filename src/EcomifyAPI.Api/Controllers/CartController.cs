using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[Route("api/v1/carts")]
[ApiController]
[ServiceFilter(typeof(ResultFilter))]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Get the cart of a user
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <returns>A <see cref="CartResponseDTO"/> containing the cart of the user</returns>
    /// <response code="200">Returns the cart of the user. If the cart is empty, creates a new one.</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="404">The user was not found</response>
    /// <response code="422">Validation errors</response>
    [Authorize]
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCart(string userId)
    {
        var result = await _cartService.GetCartAsync(userId);

        return result.Match(
            onSuccess: (cart) => Ok(cart),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Add an item to the cart of a user
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="request">The request containing the product id and the quantity</param>
    /// <returns>A <see cref="CartResponseDTO"/> containing the updated cart</returns>
    /// <response code="200">Returns the updated cart</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="404">The product was not found</response>
    /// <response code="422">Validation errors</response>
    [Authorize]
    [HttpPost("{userId}")]
    public async Task<IActionResult> AddItem(string userId, [FromBody] AddItemRequestDTO request)
    {
        var result = await _cartService.AddItemAsync(userId, request.ProductId, request.Quantity);

        return result.Match(
            onSuccess: (cart) => Ok(cart),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Apply a coupon to the cart of a user
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="request">The request containing the coupon code</param>
    /// <returns>A <see cref="CartResponseDTO"/> containing the updated cart</returns>
    /// <response code="200">Returns the updated cart</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="404">The product was not found</response>
    [Authorize]
    [HttpPost("{userId}/coupons")]
    public async Task<IActionResult> ApplyCoupon(string userId, [FromBody] ApplyDiscountRequestDTO request)
    {
        var result = await _cartService.ApplyDiscountAsync(userId, request);

        return result.Match(
            onSuccess: (cart) => Ok(cart),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Update the quantity of an item in the cart of a user
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="request">The request containing the product id and the quantity</param>
    /// <returns>A <see cref="CartResponseDTO"/> containing the updated cart</returns>
    /// <response code="200">Returns the updated cart</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="404">The product was not found</response>
    [Authorize]
    [HttpPut("{userId}/items")]
    public async Task<IActionResult> UpdateItemQuantity(string userId, [FromBody] UpdateItemQuantityRequestDTO request)
    {
        var result = await _cartService.UpdateItemQuantityAsync(userId, request.ProductId, request.Quantity);

        return result.Match(
            onSuccess: (cart) => Ok(cart),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Remove an item from the cart of a user
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="productId">The product's id</param>
    /// <returns>A <see cref="CartResponseDTO"/> containing the updated cart</returns>
    /// <response code="200">Returns the updated cart</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="404">The product was not found</response>
    [Authorize]
    [HttpDelete("{userId}/{productId}")]
    public async Task<IActionResult> RemoveItem(string userId, Guid productId)
    {
        var result = await _cartService.RemoveItemAsync(userId, productId);

        return result.Match(
            onSuccess: (cart) => Ok(cart),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Clear the cart of a user
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <returns>A <see cref="CartResponseDTO"/> containing the updated cart</returns>
    /// <response code="200">Returns the updated cart</response>
    /// <response code="401">Returned when the user is not authenticated.</response>
    /// <response code="404">The user was not found</response>
    [Authorize]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> ClearCart(string userId)
    {
        var result = await _cartService.ClearCartAsync(userId);

        return result.Match(
            onSuccess: (cart) => Ok(cart),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}