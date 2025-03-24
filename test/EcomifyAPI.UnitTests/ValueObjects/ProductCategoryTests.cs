using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.ValueObjects;

public class ProductCategoryTests
{
    private readonly ProductCategoryBuilder _builder;

    public ProductCategoryTests()
    {
        _builder = new ProductCategoryBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var productCategory = _builder.Build();

        // Assert
        productCategory.ProductId.ShouldNotBe(Guid.Empty);
        productCategory.CategoryId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldFail_WhenProductIdIsEmpty()
    {
        // Arrange
        Should.Throw<ArgumentException>(() => _builder.WithProductId(Guid.Empty).Build())
        .Message.ShouldBe("ProductId is required (Parameter 'productId')");
    }

    [Fact]
    public void Create_ShouldFail_WhenCategoryIdIsEmpty()
    {
        // Arrange
        Should.Throw<ArgumentException>(() => _builder.WithCategoryId(Guid.Empty).Build())
        .Message.ShouldBe("CategoryId is required (Parameter 'categoryId')");
    }
}