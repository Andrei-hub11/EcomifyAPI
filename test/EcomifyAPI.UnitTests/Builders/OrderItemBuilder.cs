using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class OrderItemBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _productId = Guid.NewGuid();
    private int _quantity = 1;
    private Currency _unitPrice = new("USD", 100);


    public OrderItemBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public OrderItemBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public OrderItemBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public OrderItemBuilder WithUnitPrice(Currency unitPrice)
    {
        _unitPrice = unitPrice;
        return this;
    }

    public OrderItem Build()
    {
        return new OrderItem(_id, _productId, _quantity, _unitPrice);
    }

}