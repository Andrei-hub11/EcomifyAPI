using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Services.Products;

public sealed class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;
    private readonly ILoggerHelper<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILoggerHelper<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _productRepository = unitOfWork.GetRepository<IProductRepository>();
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<ProductResponseDTO>>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _productRepository.GetProductsAsync(cancellationToken);

            return Result.Ok(products.ToResponseDTO());
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<Result<ProductResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(id, cancellationToken);


            if (product is null)
            {
                return Result.Fail(ProductErrorFactory.ProductNotFoundById(id));
            }

            var productCategories = await _productRepository.GetProductCategoryByIdAsync(id, cancellationToken);

            product.ProductCategories = [.. productCategories];

            return product.ToResponseDTO();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<bool>> CreateAsync(CreateProductRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.CurrencyCode,
            request.Stock,
            request.ImageUrl,
            request.Status.ToProductStatusDomain());

            if (product.IsFailure)
            {
                return Result.Fail(product.Errors);
            }

            var productId = await _productRepository.CreateProductAsync(product.Value, cancellationToken);

            foreach (var categoryId in request.Categories)
            {
                var category = await _productRepository.GetCategoryByIdAsync(categoryId, cancellationToken);

                if (category is null)
                {
                    return Result.Fail(ProductErrorFactory.CategoryNotFoundById(categoryId));
                }

                await _productRepository.CreateProductCategoryAsync(new ProductCategory(productId, categoryId), cancellationToken);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<bool>> CreateCategoryAsync(CreateCategoryRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = Category.Create(
            Guid.NewGuid(),
            request.Name,
            request.Description);

            if (category.IsFailure)
            {
                return Result.Fail(category.Errors);
            }

            await _productRepository.CreateCategoryAsync(category.Value, cancellationToken);

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<bool>> UpdateAsync(UpdateProductRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productRepository.GetProductByIdAsync(request.Id, cancellationToken);

            if (product is null)
            {
                return Result.Fail(ProductErrorFactory.ProductNotFoundById(request.Id));
            }

            var updatedProduct = product.ToDomain();

            await _productRepository.UpdateProductAsync(updatedProduct, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}