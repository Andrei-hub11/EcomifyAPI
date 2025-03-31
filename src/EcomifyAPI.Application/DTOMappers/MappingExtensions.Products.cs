using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsProducts
{
    public static ProductStatusEnum ToProductStatusDomain(this ProductStatusDTO status)
    {
        return status switch
        {
            ProductStatusDTO.Active => ProductStatusEnum.Active,
            ProductStatusDTO.Inactive => ProductStatusEnum.Inactive,
            ProductStatusDTO.Discontinued => ProductStatusEnum.Discontinued,
            ProductStatusDTO.OutOfStock => ProductStatusEnum.OutOfStock,
            _ => throw new ArgumentException("Invalid product status"),
        };
    }

    public static ProductStatusDTO ToProductStatusDTO(this ProductStatusEnum status)
    {
        return status switch
        {
            ProductStatusEnum.Active => ProductStatusDTO.Active,
            ProductStatusEnum.Inactive => ProductStatusDTO.Inactive,
            ProductStatusEnum.Discontinued => ProductStatusDTO.Discontinued,
            ProductStatusEnum.OutOfStock => ProductStatusDTO.OutOfStock,
            _ => throw new ArgumentException("Invalid product status"),
        };
    }

    public static Product ToDomain(this ProductMapping product)
    {
        var result = Product.From(product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.CurrencyCode,
        product.Stock,
        product.ImageUrl,
        product.Status.ToProductStatusDomain());

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Description);
        }

        result.Value.UpdateCategories(product.ProductCategories.Select(category => new ProductCategory(category.ProductId, category.ProductCategoryId)).ToList());

        return result.Value;
    }

    public static Product ToDomain(this ProductResponseDTO product)
    {
        var result = Product.From(product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.CurrencyCode,
        product.Stock,
        product.ImageUrl,
        product.Status.ToProductStatusDomain());

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
        product.Status.ToProductStatusDTO(),
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