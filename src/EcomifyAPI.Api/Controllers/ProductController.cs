using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[Route("api/v1/products")]
[ApiController]
[ServiceFilter(typeof(ResultFilter))]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Retrieves all products.
    /// </summary>
    /// <returns>
    /// A list of <see cref="ProductResponseDTO"/> containing all products.
    /// </returns>
    /// <response code="200">Returns the list of products when found successfully.</response>
    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var result = await _productService.GetAsync();

        return result.Match(
            onSuccess: (products) => Ok(products),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>
    /// A <see cref="ProductResponseDTO"/> containing the product details if found.
    /// </returns>
    /// <response code="200">Returns the product details when found successfully.</response>
    /// <response code="404">Returned when no product with the specified ID exists.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);

        return result.Match(
            onSuccess: (product) => Ok(product),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Retrieves products with low stock.
    /// </summary>
    /// <param name="request">The request containing the filter parameters.</param>
    /// <returns>
    /// A list of <see cref="ProductResponseDTO"/> containing the products with low stock.
    /// </returns>
    /// <response code="200">Returns the list of products with low stock when found successfully.</response>
    /// <response code="401">Returns the error if the user is not authenticated.</response>
    /// <response code="403">Returns the error if the user is not authorized to access the resource.</response>
    /// <response code="422">Returns the validation errors if the request is not valid.</response>
    [Authorize(Policy = "Admin")]
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] ProductFilterRequestDTO request)
    {
        var result = await _productService.GetLowStockProductsAsync(request);

        return result.Match(
            onSuccess: (products) => Ok(products),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <returns>
    /// A list of <see cref="CategoryResponseDTO"/> containing all categories.
    /// </returns>
    /// <response code="200">Returns the list of categories when found successfully.</response>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _productService.GetCategoriesAsync();

        return result.Match(
            onSuccess: (categories) => Ok(categories),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The request containing the product details.</param>
    /// <returns>
    /// A boolean value indicating whether the product was created successfully.
    /// </returns>
    /// <response code="200">Returns true if the product was created successfully.</response>
    /// <response code="401">Returns the error if the user is not authenticated.</response>
    /// <response code="403">Returns the error if the user is not authorized to access the resource.</response>
    /// <response code="422">Returns the validation errors if the product is not valid.</response>
    [Authorize(Policy = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequestDTO request)
    {
        var result = await _productService.CreateAsync(request);

        return result.Match(
            onSuccess: (isCreated) => Ok(isCreated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="request">The request containing the category details.</param>
    /// <returns>
    /// A boolean value indicating whether the category was created successfully.
    /// </returns>
    /// <response code="200">Indicates that the category was created successfully.</response>
    /// <response code="401">Returns the error if the user is not authenticated.</response>
    /// <response code="403">Returns the error if the user is not authorized to access the resource.</response>
    /// <response code="422">Returns the validation errors if the category is not valid.</response>
    [Authorize(Policy = "Admin")]
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequestDTO request)
    {
        var result = await _productService.CreateCategoryAsync(request);

        return result.Match(
            onSuccess: (isCreated) => Ok(isCreated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Updates a product.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="request">The request containing the product details.</param>
    /// <returns>
    /// A boolean value indicating whether the product was updated successfully.
    /// </returns>
    /// <response code="200">Indicates that the product was updated successfully.</response>
    /// <response code="401">Returns the error if the user is not authenticated.</response>
    /// <response code="403">Returns the error if the user is not authorized to access the resource.</response>
    /// <response code="404">Returns the error if the product was not found.</response>
    /// <response code="422">Returns the validation errors if the product is not valid.</response>
    [Authorize(Policy = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, UpdateProductRequestDTO request)
    {
        var result = await _productService.UpdateAsync(id, request);

        return result.Match(
            onSuccess: (isUpdated) => Ok(isUpdated),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>
    /// A boolean value indicating whether the product was deleted successfully.
    /// </returns>
    /// <response code="200">Indicates that the product was deleted successfully.</response>
    /// <response code="401">Returns the error if the user is not authenticated.</response>
    /// <response code="403">Returns the error if the user is not authorized to access the resource.</response>
    /// <response code="404">Returns the error if the product was not found.</response>
    [Authorize(Policy = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var result = await _productService.DeleteAsync(id);

        return result.Match(
            onSuccess: (isDeleted) => Ok(isDeleted),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}