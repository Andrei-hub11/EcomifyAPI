using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class ProductCategoryBuilder
{
    private Guid _productId = Guid.NewGuid();
    private Guid _categoryId = Guid.NewGuid();

    public ProductCategoryBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public ProductCategoryBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ProductCategory Build()
    {
        return new ProductCategory(_productId, _categoryId);
    }
}