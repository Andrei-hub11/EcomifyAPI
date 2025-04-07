using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class ProductRequestBuilder
{
    private string _name = $"Product {Guid.NewGuid()}";
    private string _description = "Description of the product";
    private decimal _price = 199.99m;
    private string _currencyCode = "BRL";
    private int _stock = 10;
    private string _imageUrl = "https://example.com/image.png";
    private ProductStatusDTO _status = ProductStatusDTO.Active;
    private HashSet<Guid> _categories = new();

    public ProductRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ProductRequestBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ProductRequestBuilder WithCurrencyCode(string currencyCode)
    {
        _currencyCode = currencyCode;
        return this;
    }

    public ProductRequestBuilder WithStock(int stock)
    {
        _stock = stock;
        return this;
    }

    public ProductRequestBuilder WithImageUrl(string imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public ProductRequestBuilder WithStatus(ProductStatusDTO status)
    {
        _status = status;
        return this;
    }

    public ProductRequestBuilder WithCategories(params Guid[] categoryIds)
    {
        _categories = categoryIds.ToHashSet();
        return this;
    }

    public CreateProductRequestDTO Build()
    {
        return new CreateProductRequestDTO(
            _name,
            _description,
            _price,
            _currencyCode,
            _stock,
            _imageUrl,
            _status,
            _categories
        );
    }
}