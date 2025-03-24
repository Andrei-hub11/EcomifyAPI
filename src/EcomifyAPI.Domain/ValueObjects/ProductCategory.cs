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
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("ProductId is required", nameof(productId));
        }

        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("CategoryId is required", nameof(categoryId));
        }
    }
};