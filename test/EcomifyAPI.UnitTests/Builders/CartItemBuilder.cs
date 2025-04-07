using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class CartItemBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _productId = Guid.NewGuid();
    private int _quantity = 1;
    private Money _unitPrice = new("BRL", 10);

    public CartItemBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CartItemBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public CartItemBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public CartItemBuilder WithUnitPrice(Money unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public CartItem Build()
    {
        return new CartItem(_productId, _quantity, _unitPrice, _id);
    }
}