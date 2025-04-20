using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct CartDiscount
{
    public Guid DiscountId { get; init; }
    public Money Amount { get; init; }
    public DiscountType DiscountType { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime ValidTo { get; init; }
    public bool IsValid => ValidFrom <= DateTime.UtcNow && ValidTo >= DateTime.UtcNow;

    public CartDiscount(Guid discountId, Money amount, DiscountType discountType, DateTime validFrom, DateTime validTo)
    {
        var errors = ValidateCartDiscount(discountId, amount, discountType, validFrom, validTo);

        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }

        DiscountId = discountId;
        Amount = amount;
        DiscountType = discountType;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    private static ReadOnlyCollection<ValidationError> ValidateCartDiscount(
        Guid id, Money amount, DiscountType discountType, DateTime validFrom, DateTime validTo)
    {
        var errors = new List<ValidationError>();

        if (id == Guid.Empty)
        {
            errors.Add(Error.Validation("Id is required", "ERR_ID_REQ", "id"));
        }

        if (amount.Amount <= 0)
        {
            errors.Add(Error.Validation("Amount must be greater than 0", "ERR_AMOUNT_GT_0", "amount"));
        }

        if (!Enum.IsDefined(typeof(DiscountType), discountType))
        {
            errors.Add(Error.Validation("Invalid discount type", "ERR_TYPE_INV", "discountType"));
        }

        return errors.AsReadOnly();
    }
}