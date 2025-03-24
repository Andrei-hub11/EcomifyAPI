using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsProducts
{
    public static Product ToDomain(this ProductMapping product)
    {
        var result = Product.Create(product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.Stock,
        product.ImageUrl,
        product.Status);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Description);
        }

        result.Value.UpdateCategories(product.Categories.Select(category => new ProductCategory(category.ProductId, category.CategoryId)).ToList());

        return result.Value;
    }

    public static Product ToDomain(this ProductResponseDTO product)
    {
        var result = Product.Create(product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.Stock,
        product.ImageUrl,
        product.Status);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Description);
        }

        result.Value.UpdateCategories(product.Categories.Select(category => new ProductCategory(category.ProductId, category.CategoryId)).ToList());

        return result.Value;
    }

    public static ProductResponseDTO ToResponseDTO(this Product product)
    {
        return new ProductResponseDTO(product.Id,
        product.Name,
        product.Description,
        product.Price.Amount,
        product.Stock,
        product.ImageUrl,
        product.Status,
        [.. product.Categories.Select(category => new ProductCategoryResponseDTO(category.ProductId, category.CategoryId))]);
    }

    public static ProductResponseDTO ToResponseDTO(this ProductMapping product)
    {
        return new ProductResponseDTO(product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.Stock,
        product.ImageUrl,
        product.Status,
        [.. product.Categories.Select(category => new ProductCategoryResponseDTO(category.ProductId, category.CategoryId))]);
    }
}