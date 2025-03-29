using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public sealed class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Currency UnitPrice { get; private set; }
    public Currency TotalPrice => new(UnitPrice.Code, UnitPrice.Amount * Quantity);

    public OrderItem(Guid id, Guid productId, int quantity, Currency unitPrice)
    {
        var errors = ValidateOrderItem(id, productId, quantity, unitPrice);

        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }

        Id = id;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    private static ReadOnlyCollection<ValidationError> ValidateOrderItem(Guid id, Guid productId, int quantity, Currency unitPrice)
    {
        var errors = new List<ValidationError>();

        if (id == Guid.Empty)
        {
            errors.Add(ValidationError.Create("Id is required", "ERR_ID_REQUIRED", "Id"));
        }

        if (productId == Guid.Empty)
        {
            errors.Add(ValidationError.Create("ProductId is required", "ERR_PRODUCT_ID_REQUIRED", "ProductId"));
        }

        if (quantity <= 0)
        {
            errors.Add(ValidationError.Create("Quantity must be greater than 0", "ERR_QTY_GT_0", "Quantity"));
        }

        if (unitPrice.Amount <= 0)
        {
            errors.Add(ValidationError.Create("UnitPrice must be greater than 0", "ERR_UPRICE_GT_0", "UnitPrice"));
        }

        return errors.AsReadOnly();
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException(Error.Validation("Quantity must be greater than 0", "ERR_QTY_GT_0", "Quantity"));
        }

        Quantity = quantity;
    }
};