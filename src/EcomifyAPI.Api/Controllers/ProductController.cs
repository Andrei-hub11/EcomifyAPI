using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Api.Middleware;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

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
        var result = await _productService.GetProductsAsync();

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
        var result = await _productService.GetProductByIdAsync(id);

        return result.Match(
            onSuccess: (product) => Ok(product),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequestDTO request)
    {
        var result = await _productService.CreateProductAsync(request);

        return result.Match(
            onSuccess: (product) => Ok(product),
            onFailure: (errors) => errors.ToProblemDetailsResult()
        );
    }
}