using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Validation;
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
            var products = await _productRepository.GetAsync(cancellationToken);

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
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);


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

    public async Task<Result<PaginatedResponseDTO<ProductResponseDTO>>> GetLowStockProductsAsync(ProductFilterRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            var errors = FilterDTOValidation.Validate(
                request.StockThreshold,
                request.PageSize,
                request.PageNumber,
                request.Name,
                request.Category);

            if (errors.Any())
            {
                return Result.Fail(errors);
            }

            var productsFiltered = await _productRepository.GetLowStockProductsAsync(request, cancellationToken);

            return Result.Ok(productsFiltered.ToResponseDTO(request.PageSize, request.PageNumber));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _productRepository.GetCategoriesAsync(cancellationToken);

            return Result.Ok(categories.ToResponseDTO());
        }
        catch
        {
            throw;
        }
    }

    public async Task<Result<CategoryResponseDTO>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _productRepository.GetCategoryByIdAsync(id, cancellationToken);

            if (category is null)
            {
                return Result.Fail(ProductErrorFactory.CategoryNotFoundById(id));
            }

            return category.ToResponseDTO();
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

            var productId = await _productRepository.CreateAsync(product.Value, cancellationToken);

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
            await _unitOfWork.RollbackAsync();
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

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<bool>> UpdateAsync(Guid id, UpdateProductRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(id, cancellationToken);

            if (existingProduct is null)
            {
                return Result.Fail(ProductErrorFactory.ProductNotFoundById(id));
            }

            var product = Product.From(
            existingProduct.Id,
            existingProduct.Name,
            existingProduct.Description,
            existingProduct.Price,
            existingProduct.CurrencyCode,
            existingProduct.Stock,
            existingProduct.ImageUrl,
            existingProduct.Status.ToProductStatusDomain());

            if (product.IsFailure)
            {
                return Result.Fail(product.Errors);
            }

            bool isChanged = false;

            isChanged |= product.Value.UpdateName(request.Name);

            isChanged |= product.Value.UpdateDescription(request.Description);

            isChanged |= product.Value.UpdatePrice(request.Price, request.CurrencyCode);

            isChanged |= product.Value.UpdateStock(request.Stock);

            isChanged |= product.Value.UpdateImageUrl(request.ImageUrl);

            isChanged |= product.Value.UpdateStatus(request.Status.ToProductStatusDomain());

            foreach (var categoryId in request.Categories.CategoryIds)
            {
                isChanged |= product.Value.UpdateCategories([new ProductCategory(id, categoryId)]);
            }

            if (isChanged)
            {
                await _productRepository.UpdateProductAsync(product.Value, cancellationToken);
                await _productRepository.DeleteProductCategoryAsync(id,
                [.. existingProduct.Categories.Select(c => c.CategoryId)], cancellationToken);

                foreach (var category in product.Value.ProductCategories)
                {
                    await _productRepository.CreateProductCategoryAsync(category, cancellationToken);
                }

                await _unitOfWork.CommitAsync(cancellationToken);
            }

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);

            if (product is null)
            {
                return Result.Fail(ProductErrorFactory.ProductNotFoundById(id));
            }

            await _productRepository.DeleteAsync(id, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}