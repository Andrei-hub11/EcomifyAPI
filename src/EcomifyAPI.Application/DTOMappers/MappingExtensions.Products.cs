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
        product.CurrencyCode,
        product.Stock,
        product.ImageUrl,
        product.Status);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Description);
        }

        result.Value.UpdateCategories(product.ProductCategories.Select(category => new ProductCategory(category.ProductId, category.ProductCategoryId)).ToList());

        return result.Value;
    }

    public static Product ToDomain(this ProductResponseDTO product)
    {
        var result = Product.Create(product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.CurrencyCode,
        product.Stock,
        product.ImageUrl,
        product.Status);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Description);
        }

        return result.Value;
    }

    public static ProductResponseDTO ToResponseDTO(this Product product, List<CategoryMapping> categories)
    {
        return new ProductResponseDTO(product.Id,
        product.Name,
        product.Description,
        product.Price.Amount,
        product.Price.Code,
        product.Stock,
        product.ImageUrl,
        product.Status,
        [.. categories.Where(category => product.ProductCategories.Any(pc => pc.CategoryId == category.CategoryId))
        .Select(category => new CategoryResponseDTO(category.CategoryId, category.CategoryName, category.CategoryDescription))]);
    }

    public static ProductResponseDTO ToResponseDTO(this ProductMapping product)
    {
        return new ProductResponseDTO(product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.CurrencyCode,
        product.Stock,
        product.ImageUrl,
        product.Status,
        [.. product.Categories.Select(category => new CategoryResponseDTO(category.CategoryId, category.CategoryName, category.CategoryDescription))]);
    }

    public static IReadOnlyList<ProductResponseDTO> ToResponseDTO(this IEnumerable<ProductMapping> products)
    {
        return products.Select(product => product.ToResponseDTO()).ToList();
    }
}