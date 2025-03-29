using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.UnitTests.Builders;

public class ProductBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test Product";
    private readonly string _description = "Test Description";
    private decimal _price = 100.00m;
    private string _currencyCode = "BRL";
    private int _stock = 10;
    private readonly string _imageUrl = "http://example.com/image.jpg";
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

    public Result<Product> Build()
    {
        return Product.Create(
            _id,
            _name,
            _description,
            _price,
            _currencyCode,
            _stock,
            _imageUrl,
            _status);
    }
}