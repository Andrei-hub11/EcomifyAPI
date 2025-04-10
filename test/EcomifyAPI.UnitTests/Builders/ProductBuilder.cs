using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.UnitTests.Builders;

public class ProductBuilder
{
    private Guid? _id = Guid.NewGuid();
    private string _name = "Test Product";
    private string _description = "Test Description";
    private decimal _price = 100.00m;
    private string _currencyCode = "BRL";
    private int _stock = 10;
    private string _imageUrl = "http://example.com/image.jpg";
    private ProductStatusEnum _status = ProductStatusEnum.Active;

    public ProductBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ProductBuilder WithCurrencyCode(string currencyCode)
    {
        _currencyCode = currencyCode;
        return this;
    }

    public ProductBuilder WithStock(int stock)
    {
        _stock = stock;
        return this;
    }

    public ProductBuilder WithStatus(ProductStatusEnum status)
    {
        _status = status;
        return this;
    }

    public ProductBuilder WithImageUrl(string imageUrl)
    {
        _imageUrl = imageUrl;
        return this;
    }

    public ProductBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Result<Product> Build()
    {
        return Product.Create(
            _name,
            _description,
            _price,
            _currencyCode,
            _stock,
            _imageUrl,
            _status,
            _id
            );
    }

    public Result<Product> BuildFrom()
    {
        return Product.From(
            _id ?? Guid.NewGuid(),
            _name,
            _description,
            _price,
            _currencyCode,
            _stock, _imageUrl, _status);
    }
}