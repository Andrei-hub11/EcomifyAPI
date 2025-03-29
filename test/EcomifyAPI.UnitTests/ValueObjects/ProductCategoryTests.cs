using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;
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
        Should.Throw<DomainException>(() => _builder.WithProductId(Guid.Empty).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_PRODUCT_ID_REQUIRED"
        && e.Description == "ProductId is required" && e.ErrorType == ErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenCategoryIdIsEmpty()
    {
        // Arrange
        Should.Throw<DomainException>(() => _builder.WithCategoryId(Guid.Empty).Build())
        .Errors.ShouldContain(e => e.Code == "ERR_CATEGORY_ID_REQUIRED"
        && e.Description == "CategoryId is required" && e.ErrorType == ErrorType.Validation);
    }
}