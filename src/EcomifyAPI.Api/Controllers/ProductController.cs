using EcomifyAPI.Api.Extensions;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Contracts.Request;

using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

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