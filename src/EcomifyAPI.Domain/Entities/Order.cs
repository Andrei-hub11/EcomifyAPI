using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public Money TotalAmount => CalculateTotalAmount();
    public DateTime OrderDate { get; private set; }
    public OrderStatusEnum Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Address ShippingAddress { get; private set; }
    public Address BillingAddress { get; private set; }

    public IReadOnlyList<OrderItem> OrderItems => _items.AsReadOnly();
    private readonly List<OrderItem> _items = [];

    private Order(
        Guid id,
        string userId,
        DateTime orderDate,
        OrderStatusEnum status,
        DateTime createdAt,
        DateTime? completedAt,
        Address shippingAddress,
        Address billingAddress,
        List<OrderItem> orderItems)
    {
        Id = id;
        UserId = userId;
        OrderDate = orderDate;
        Status = status;
        CreatedAt = createdAt;
        CompletedAt = completedAt;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        _items.AddRange(orderItems);
    }

    public static Result<Order> Create(
        string userId,
        DateTime orderDate,
        OrderStatusEnum status,
        DateTime createdAt,
        DateTime? completedAt,
        Address shippingAddress,
        Address billingAddress,
        Guid? id = null
        )
    {
        var errors = ValidateOrder(userId, orderDate, status, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Order(
        id ?? Guid.Empty,
        userId, orderDate,
        status, createdAt,
        completedAt,
        shippingAddress,
        billingAddress,
        []);
    }

    public static Result<Order> From(
        Guid id,
        string userId,
        DateTime orderDate,
        OrderStatusEnum status,
        DateTime createdAt,
        DateTime? completedAt,
        Address shippingAddress,
        Address billingAddress,
        List<OrderItem> items)
    {
        var errors = ValidateOrder(userId, orderDate, status, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Order(
            id,
            userId,
            orderDate,
            status,
            createdAt,
            completedAt,
            shippingAddress,
            billingAddress,
            items);
    }

    private static ReadOnlyCollection<ValidationError> ValidateOrder(
        string userId,
        DateTime orderDate,
        OrderStatusEnum status,
        Guid? id = null
        )
    {
        var errors = new List<ValidationError>();

        if (id is not null && id == Guid.Empty)
        {
            errors.Add(ValidationError.Create("Id is required", "ERR_ID_REQUIRED", "Id"));
        }

        if (string.IsNullOrEmpty(userId))
        {
            errors.Add(ValidationError.Create("UserId is required", "ERR_USER_ID_REQUIRED", "UserId"));
        }

        if (orderDate == DateTime.MinValue)
        {
            errors.Add(ValidationError.Create("OrderDate is required", "ERR_ORDER_DATE_REQUIRED", "OrderDate"));
        }

        if (status == OrderStatusEnum.Pending)
        {
            errors.Add(ValidationError.Create("Status must be pending", "ERR_STATUS_MUST_BE_PENDING", "Status"));
        }

        return errors.AsReadOnly();
    }


    public void AddItem(Product product, int quantity, Money unitPrice)
    {
        if (Status != OrderStatusEnum.Created)
        {
            throw new InvalidOperationException("Order is alredy being processed");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == product.Id);

        if (item != null)
        {
            item.UpdateQuantity(item.Quantity + quantity);
        }

        _items.Add(new OrderItem(Guid.NewGuid(), product.Id, quantity, unitPrice));
    }


    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatusEnum.Created)
        {
            throw new InvalidOperationException("Order is alredy being processed");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
        {
            throw new InvalidOperationException("Item not found");
        }

        _items.Remove(item);
    }

    public void ProcessPayment()
    {
        if (Status != OrderStatusEnum.Created)
        {
            throw new InvalidOperationException("Order is alredy being processed");
        }

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("Empty order cannot be processed");
        }

        UpdateStatus(OrderStatusEnum.Processing);
    }

    public void ConfirmPayment()
    {
        if (Status != OrderStatusEnum.Processing)
        {
            throw new InvalidOperationException("Processing order cannot be confirmed");
        }

        UpdateStatus(OrderStatusEnum.Confirmed);
    }

    public void FailPayment()
    {
        if (Status != OrderStatusEnum.Processing)
        {
            throw new InvalidOperationException("Processing order cannot be failed");
        }

        UpdateStatus(OrderStatusEnum.Cancelled);
    }

    public void ShipOrder()
    {
        if (Status != OrderStatusEnum.Confirmed)
        {
            throw new InvalidOperationException("Confirmed order cannot be shipped");
        }

        UpdateStatus(OrderStatusEnum.Shipped);
    }

    public void CompleteOrder()
    {
        if (Status != OrderStatusEnum.Shipped)
        {
            throw new InvalidOperationException("Shipped order cannot be completed");
        }

        UpdateStatus(OrderStatusEnum.Completed);

        CompletedAt = DateTime.UtcNow;
    }

    public void CancelOrder()
    {
        if (Status == OrderStatusEnum.Shipped || Status == OrderStatusEnum.Completed)
        {
            throw new InvalidOperationException("Order cannot be cancelled. The order is already completed");
        }

        UpdateStatus(OrderStatusEnum.Cancelled);
    }

    public void UpdateStatus(OrderStatusEnum status)
    {
        Status = status;
    }

    public void UpdateShippingAddress(Address shippingAddress)
    {
        ShippingAddress = shippingAddress;
    }

    public void UpdateBillingAddress(Address billingAddress)
    {
        BillingAddress = billingAddress;
    }

    private Money CalculateTotalAmount()
    {
        var totalAmount = _items.Sum(i => i.TotalPrice.Amount);
        var currencyCode = _items.First().TotalPrice.Code;

        if (totalAmount <= 0)
        {
            throw new InvalidOperationException("Total amount must be greater than 0");
        }

        return new Money(currencyCode, totalAmount);
    }
}