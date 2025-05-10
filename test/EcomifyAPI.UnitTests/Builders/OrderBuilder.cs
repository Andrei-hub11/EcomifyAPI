using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class OrderBuilder
{
    private Guid? _id = Guid.NewGuid();
    private string _userId = "user123";
    private DateTime _orderDate = DateTime.UtcNow;
    private OrderStatusEnum _status = OrderStatusEnum.Confirmed;
    private readonly DateTime _createdAt = DateTime.UtcNow;
    private readonly DateTime? _completedAt = null;
    private DateTime? _shippedAt = null;
    private Address _shippingAddress;
    private Address _billingAddress;
    private decimal _discountAmount = 0;
    private string _currencyCode = "USD";

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

    public OrderBuilder WithId(Guid? id)
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
        if (status == OrderStatusEnum.Shipped)
        {
            _shippedAt = DateTime.UtcNow;
        }
        return this;
    }

    public OrderBuilder AsConfirmed()
    {
        _status = OrderStatusEnum.Confirmed;
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

    public OrderBuilder WithDiscountAmount(decimal discountAmount)
    {
        _discountAmount = discountAmount;
        return this;
    }

    public OrderBuilder WithCurrencyCode(string currencyCode)
    {
        _currencyCode = currencyCode;
        return this;
    }

    public OrderBuilder WithShippedAt(DateTime? shippedAt)
    {
        _shippedAt = shippedAt;
        return this;
    }

    public Result<Order> Build()
    {
        var order = Order.Create(
            _userId,
            _orderDate,
            _createdAt,
            _completedAt,
            _shippingAddress,
            _billingAddress,
            _id,
            _discountAmount
            );

        return order;
    }

    // Helper method to create a test order with items
    public Result<Order> BuildWithItems(int numberOfItems = 1, decimal pricePerItem = 100m)
    {
        var order = Build();

        if (order.IsFailure)
        {
            return order;
        }

        var product = Product.Create(
            "Test Product",
            "Test Description",
            pricePerItem,
            _currencyCode,
            100, // stock
            "http://example.com/image.jpg",
            ProductStatusEnum.Active,
            Guid.NewGuid()
        );

        if (product.IsFailure)
        {
            return Result.Fail(product.Errors);
        }

        for (int i = 0; i < numberOfItems; i++)
        {
            order.Value.AddItem(product.Value, 1, new Money(_currencyCode, pricePerItem));
        }

        return order;
    }

    public Result<Order> BuildFrom()
    {
        var order = Build();

        if (order.IsFailure)
        {
            return order;
        }

        var newOrder = Order.From(
            order.Value.Id,
            order.Value.UserId,
            order.Value.OrderDate,
            order.Value.Status,
            order.Value.CreatedAt,
            order.Value.CompletedAt,
            order.Value.ShippingAddress,
            order.Value.BillingAddress,
            [.. order.Value.OrderItems],
            order.Value.DiscountAmount,
            order.Value.TotalWithDiscount,
           _shippedAt
        );

        return newOrder;
    }
}