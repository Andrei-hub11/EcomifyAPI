using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct ProductCategory
{
    public Guid ProductId { get; init; }
    public Guid CategoryId { get; init; }

    public ProductCategory(Guid productId, Guid categoryId)
    {
        ValidateProductCategory(productId, categoryId);
        ProductId = productId;
        CategoryId = categoryId;
    }

    private static void ValidateProductCategory(Guid productId, Guid categoryId)
    {
        var errors = new List<IError>();

        if (productId == Guid.Empty)
        {
            errors.Add(Error.Validation("ProductId is required", "ERR_PRODUCT_ID_REQUIRED", "ProductId"));
        }

        if (categoryId == Guid.Empty)
        {
            errors.Add(Error.Validation("CategoryId is required", "ERR_CATEGORY_ID_REQUIRED", "CategoryId"));
        }

        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }
    }
};