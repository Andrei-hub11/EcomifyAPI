using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class UpdateProductRequestBuilder
{
    private string _name = "Test Product";
    private string _description = "Test Description";
    private decimal _price = 100.00m;
    private readonly string _currencyCode = "BRL";
    private int _stock = 10;
    private string _imageUrl = "http://example.com/image.jpg";
    private ProductStatusDTO _status = ProductStatusDTO.Active;
    private Guid[] _categoryIds = [];

    public UpdateProductRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UpdateProductRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public UpdateProductRequestBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public UpdateProductRequestBuilder WithStock(int stock)
    {
        _stock = stock;
        return this;
    }

    public UpdateProductRequestBuilder WithImageUrl(string imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public UpdateProductRequestBuilder WithStatus(ProductStatusDTO status)
    {
        _status = status;
        return this;
    }

    public UpdateProductRequestBuilder WithCategories(Guid[] categoryIds)
    {
        _categoryIds = categoryIds;
        return this;
    }

    public UpdateProductRequestBuilder WithCategory(List<Guid> categoryIds)
    {
        _categoryIds = [.. categoryIds];
        return this;
    }

    public UpdateProductRequestDTO Build()
    {
        return new UpdateProductRequestDTO(
            _name,
            _description,
            _price,
            _currencyCode,
            _stock,
            _imageUrl,
            _status,
            new UpdateProductCategoryRequestDTO(_categoryIds)
        );
    }
}