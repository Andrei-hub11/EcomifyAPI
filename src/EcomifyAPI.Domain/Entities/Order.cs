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
    public Money TotalWithDiscount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatusEnum Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
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
        List<OrderItem> orderItems,
        decimal discountAmount = 0,
        Money? totalWithDiscount = null,
        DateTime? shippedAt = null)
    {
        Id = id;
        UserId = userId;
        OrderDate = orderDate;
        Status = status;
        CreatedAt = createdAt;
        CompletedAt = completedAt;
        ShippingAddress = shippingAddress;
        BillingAddress = billingAddress;
        DiscountAmount = discountAmount;
        ShippedAt = shippedAt;
        _items.AddRange(orderItems);

        if (totalWithDiscount is not null)
        {
            TotalWithDiscount = totalWithDiscount.Value;
        }
        else
        {
            // Initialize TotalWithDiscount to be the same as TotalAmount
            var totalAmount = CalculateTotalAmount();
            decimal finalAmount = TotalAmount.Amount - discountAmount;

            if (finalAmount < 0)
            {
                finalAmount = 0;
            }

            TotalWithDiscount = finalAmount > 0
                ? new Money(totalAmount.Code, finalAmount)
                : Money.Zero(totalAmount.Code);

        }
    }

    public static Result<Order> Create(
        string userId,
        DateTime orderDate,
        DateTime createdAt,
        DateTime? completedAt,
        Address shippingAddress,
        Address billingAddress,
        Guid? id = null,
        decimal discountAmount = 0
        )
    {
        var status = OrderStatusEnum.Confirmed;
        var errors = ValidateOrder(userId, orderDate, status, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Order(
        id ?? Guid.Empty,
        userId,
        orderDate,
        status,
        createdAt,
        completedAt,
        shippingAddress,
        billingAddress,
        [],
        discountAmount,
        null,
        null);
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
        List<OrderItem> items,
        decimal discountAmount = 0,
        Money? totalWithDiscount = null,
        DateTime? shippedAt = null)
    {
        var errors = ValidateOrder(userId, orderDate, status, id, shippedAt);

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
            items,
            discountAmount,
            totalWithDiscount,
            shippedAt);
    }

    private static ReadOnlyCollection<ValidationError> ValidateOrder(
        string userId,
        DateTime orderDate,
        OrderStatusEnum status,
        Guid? id = null,
        DateTime? shippedAt = null
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

        if (id is null && status != OrderStatusEnum.Confirmed)
        {
            errors.Add(ValidationError.Create("Status must be confirmed", "ERR_STATUS_MUST_BE_CONFIRMED", "Status"));
        }

        if (status == OrderStatusEnum.Shipped && shippedAt is null)
        {
            errors.Add(ValidationError.Create("ShippedAt is required", "ERR_SHIPPED_AT_REQUIRED", "ShippedAt"));
        }

        return errors.AsReadOnly();
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        if (discountAmount < 0)
        {
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));
        }

        if (discountAmount > TotalAmount.Amount)
        {
            discountAmount = TotalAmount.Amount;
        }

        DiscountAmount = discountAmount;

        var totalAmount = CalculateTotalAmount();
        decimal finalAmount = totalAmount.Amount - discountAmount;

        if (finalAmount < 0)
        {
            finalAmount = 0;
        }

        TotalWithDiscount = finalAmount > 0
            ? new Money(totalAmount.Code, finalAmount)
            : Money.Zero(totalAmount.Code);
    }

    public void AddItem(Product product, int quantity, Money unitPrice)
    {
        if (Status != OrderStatusEnum.Confirmed)
        {
            throw new InvalidOperationException("Order is already being processed");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == product.Id);

        if (item != null)
        {
            item.UpdateQuantity(item.Quantity + quantity);
        }
        else
        {
            _items.Add(new OrderItem(Guid.NewGuid(), product.Id, quantity, unitPrice));
        }

        // Recalculate total with discount

        decimal finalAmount = TotalAmount.Amount - DiscountAmount;

        if (finalAmount < 0)
        {
            finalAmount = 0;
        }

        TotalWithDiscount = finalAmount > 0
            ? new Money(TotalAmount.Code, finalAmount)
            : Money.Zero(TotalAmount.Code);
    }


    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatusEnum.Confirmed)
        {
            throw new InvalidOperationException("Order is already being processed");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
        {
            throw new InvalidOperationException("Item not found");
        }

        _items.Remove(item);

        // Recalculate total with discount
        decimal finalAmount = TotalAmount.Amount - DiscountAmount;

        if (finalAmount < 0)
        {
            finalAmount = 0;
        }

        TotalWithDiscount = finalAmount > 0
            ? new Money(TotalAmount.Code, finalAmount)
            : Money.Zero(TotalAmount.Code);
    }

    public void ShipOrder()
    {
        if (Status != OrderStatusEnum.Confirmed)
        {
            throw new InvalidOperationException("Order is not confirmed and cannot be shipped");
        }

        UpdateStatus(OrderStatusEnum.Shipped);
        ShippedAt = DateTime.UtcNow;
    }

    public void CompleteOrder()
    {
        if (Status != OrderStatusEnum.Shipped)
        {
            throw new InvalidOperationException("Order is not shipped and cannot be completed");
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
        if (_items.Count == 0)
        {
            return Money.Zero("BRL");
        }

        var totalAmount = _items.Sum(i => i.TotalPrice.Amount);
        var currencyCode = _items.First().TotalPrice.Code;

        if (totalAmount <= 0)
        {
            throw new InvalidOperationException("Total amount must be greater than 0");
        }

        return new Money(currencyCode, totalAmount);
    }
}