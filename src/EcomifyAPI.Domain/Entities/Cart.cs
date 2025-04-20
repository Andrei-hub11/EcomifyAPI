using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Domain.Entities;

public sealed class Cart
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public Money TotalAmount => CalculateTotalAmount();
    public Money TotalWithDiscount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
    public IReadOnlyList<CartDiscount> Discounts => _discounts.AsReadOnly();

    private readonly List<CartItem> _items = [];
    private readonly List<CartDiscount> _discounts = [];

    private Cart(Guid id, string userId, DateTime createdAt, DateTime? updatedAt, List<CartItem> items, List<CartDiscount> discounts)
    {
        Id = id;
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        _items = items;
        _discounts = discounts;
    }

    public static Result<Cart> Create(string userId)
    {
        var errors = ValidateCart(userId);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Cart(
            Guid.NewGuid(),
            userId,
            DateTime.UtcNow,
            null,
            [],
            []
        );
    }

    public static Result<Cart> From(
        Guid id,
        string userId,
        DateTime createdAt,
        DateTime? updatedAt,
        List<CartItem> items,
        List<CartDiscount> discounts
    )
    {
        var errors = ValidateCart(userId, createdAt, updatedAt, items, discounts, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Cart(id, userId, createdAt, updatedAt, items, discounts);
    }

    public static ReadOnlyCollection<ValidationError> ValidateCart(
        string userId,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        List<CartItem>? items = null,
        List<CartDiscount>? discounts = null,
        Guid? id = null
    )
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(userId))
        {
            errors.Add(Error.Validation("User ID is required", "ERR_USER_ID_REQUIRED", "userId"));
        }

        if (id is not null && id == Guid.Empty)
        {
            errors.Add(Error.Validation("ID is required", "ERR_ID_REQUIRED", "id"));
        }

        if (createdAt is not null && createdAt == DateTime.MinValue)
        {
            errors.Add(Error.Validation("Created at is required", "ERR_CREATED_AT_REQUIRED", "createdAt"));
        }

        if (updatedAt is not null && updatedAt == DateTime.MinValue)
        {
            errors.Add(Error.Validation("Updated at is required", "ERR_UPDATED_AT_REQUIRED", "updatedAt"));
        }

        if (items is not null)
        {
            var duplicateItems = items
                .GroupBy(i => i.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateItems.Count != 0)
            {
                errors.Add(Error.Validation("Duplicate cart items are not allowed", "ERR_DUPLICATE_CART-I", "items"));
            }
        }

        if (discounts is not null)
        {
            var duplicateDiscounts = discounts
                .GroupBy(d => d.DiscountId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateDiscounts.Count != 0)
            {
                errors.Add(Error.Validation("Duplicate discounts are not allowed", "ERR_DUPLICATE_DISC", "discounts"));
            }
        }

        return errors.AsReadOnly();
    }

    private Money CalculateTotalAmount()
    {
        if (_items.Count == 0)
        {
            return Money.Zero("BRL");
        }

        return new Money("BRL", _items.Sum(item => item.TotalPrice.Amount));
    }

    public void UpdateTotalWithDiscount(decimal discountAmount)
    {
        decimal finalAmount = TotalAmount.Amount - discountAmount;

        if (finalAmount < 0)
        {
            finalAmount = 0;
        }

        TotalWithDiscount = new Money("BRL", finalAmount);
    }

    public void AddItem(Product product, int quantity, Money unitPrice)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == product.Id);

        if (item != null)
        {
            item.UpdateQuantity(item.Quantity + quantity);
        }

        _items.Add(new CartItem(product.Id, quantity, unitPrice));
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);

        if (item == null)
        {
            throw new InvalidOperationException("Item not found");
        }

        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }
}