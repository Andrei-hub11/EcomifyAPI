using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class OrderBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _userId = "user123";
    private DateTime _orderDate = DateTime.UtcNow;
    private OrderStatusEnum _status = OrderStatusEnum.Created;
    private readonly DateTime _createdAt = DateTime.UtcNow;
    private readonly DateTime? _completedAt = null;
    private Address _shippingAddress;
    private Address _billingAddress;

    public OrderBuilder()
    {
        // Create default addresses
        var addressResult = new Address(
            "123 Main St",
            1,
            "New York",
            "NY",
            "10001",
            "United States",
            "Apt 1"
            );

        _shippingAddress = addressResult;
        _billingAddress = addressResult;
    }

    public OrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public OrderBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public OrderBuilder WithOrderDate(DateTime orderDate)
    {
        _orderDate = orderDate;
        return this;
    }

    public OrderBuilder WithStatus(OrderStatusEnum status)
    {
        _status = status;
        return this;
    }

    public OrderBuilder WithShippingAddress(Address address)
    {
        _shippingAddress = address;
        return this;
    }

    public OrderBuilder WithBillingAddress(Address address)
    {
        _billingAddress = address;
        return this;
    }

    public Result<Order> Build()
    {
        return Order.Create(
            _id,
            _userId,
            _orderDate,
            _status,
            _createdAt,
            _completedAt,
            _shippingAddress,
            _billingAddress);
    }
}