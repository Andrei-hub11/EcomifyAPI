using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[ApiController]
[Route("api/v1/discounts")]
[ServiceFilter(typeof(ResultFilter))]
public class DiscountController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    /// <summary>
    /// Get all discounts
    /// </summary>
    /// <param name="request">The request containing the discount filter details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="PaginatedResponseDTO{DiscountResponseDTO}"/> representing the discounts</returns>
    /// <response code="200">Returns the list of discounts</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="401">If the user is not authorized</response>
    /// <response code="403">If the user is not authorized to access the resource</response>
    [Authorize(Policy = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetDiscounts([FromQuery] DiscountFilterRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _discountService.GetAllAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (discounts) => Ok(discounts),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Get a discount by id
    /// </summary>
    /// <param name="id">The id of the discount to get</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="DiscountResponseDTO"/> representing the discount</returns>
    /// <response code="200">Returns the discount</response>
    /// <response code="401">If the user is not authorized</response>
    /// <response code="403">If the user is not authorized to access the resource</response>
    /// <response code="404">If the discount is not found</response>
    [Authorize(Policy = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDiscountById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _discountService.GetByIdAsync(id, cancellationToken);

        return result.Match(
            onSuccess: (discount) => Ok(discount),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Create a new discount
    /// </summary>
    /// <param name="request">The request containing the discount details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="bool"/> representing the result of the operation</returns>
    /// <response code="200">Returns the result of the operation</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="403">If the user is not authorized to access the resource</response>
    /// <response code="404">If the category is not found</response>
    /// <response code="422">Validation errors</response>
    [Authorize(Policy = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateDiscount(CreateDiscountRequestDTO request, CancellationToken cancellationToken = default)
    {
        var result = await _discountService.CreateAsync(request, cancellationToken);

        return result.Match(
            onSuccess: (isCreated) => Ok(isCreated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Deactivate a discount
    /// </summary>
    /// <param name="id">The id of the discount to deactivate</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="bool"/> representing the result of the operation</returns>
    /// <response code="200">Returns the result of the operation</response>
    /// <response code="401">If the user is not authorized</response>
    /// <response code="403">If the user is not authorized to access the resource</response>
    /// <response code="404">If the discount is not found</response>
    [Authorize(Policy = "Admin")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> DeactivateDiscount(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _discountService.DeactivateAsync(id, cancellationToken);

        return result.Match(
            onSuccess: (isDeactivated) => Ok(isDeactivated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Delete a discount
    /// </summary>
    /// <param name="id">The id of the discount to delete</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A <see cref="bool"/> representing the result of the operation</returns>
    /// <response code="200">Returns the result of the operation</response>
    /// <response code="401">If the user is not authorized</response>
    /// <response code="403">If the user is not authorized to access the resource</response>
    /// <response code="404">If the discount is not found</response>
    [Authorize(Policy = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDiscount(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _discountService.DeleteAsync(id, cancellationToken);

        return result.Match(
            onSuccess: (isDeleted) => Ok(isDeleted),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}