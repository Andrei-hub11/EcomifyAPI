using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Exceptions;

namespace EcomifyAPI.Domain.ValueObjects;

public readonly record struct DiscountApplied
{
    public Guid Id { get; init; }
    public Guid CartId { get; init; }
    public Guid CouponId { get; init; }
    public string CouponCode { get; init; }
    public DateTime CreatedAt { get; init; }

    public DiscountApplied(Guid id, Guid cartId, Guid couponId, string couponCode, DateTime createdAt)
    {
        var errors = ValidateDiscountApplied(id, cartId, couponId, couponCode, createdAt);

        if (errors.Count != 0)
        {
            throw new DomainException(errors);
        }

        Id = id;
        CartId = cartId;
        CouponId = couponId;
        CouponCode = couponCode;
        CreatedAt = createdAt;
    }

    private static ReadOnlyCollection<ValidationError> ValidateDiscountApplied(Guid id, Guid cartId, Guid couponId, string couponCode, DateTime createdAt)
    {
        var errors = new List<ValidationError>();

        if (id == Guid.Empty)
        {
            errors.Add(Error.Validation("Id is required", "ERR_ID_REQ", "id"));
        }

        if (cartId == Guid.Empty)
        {
            errors.Add(Error.Validation("CartId is required", "ERR_CART_ID_REQ", "cartId"));
        }

        if (couponId == Guid.Empty)
        {
            errors.Add(Error.Validation("CouponId is required", "ERR_COUPON_ID_REQ", "couponId"));
        }

        if (string.IsNullOrEmpty(couponCode))
        {
            errors.Add(Error.Validation("CouponCode is required", "ERR_COUPON_CODE_REQ", "couponCode"));
        }

        if (createdAt == DateTime.MinValue)
        {
            errors.Add(Error.Validation("CreatedAt is required", "ERR_CREATED_AT_REQ", "createdAt"));
        }

        return errors.AsReadOnly();
    }
}