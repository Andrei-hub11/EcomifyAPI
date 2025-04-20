using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Validation;

public static class DiscountDTOValidation
{
    public static IReadOnlyList<ValidationError> Validate(
        string couponCode,
        decimal? fixedAmount,
        decimal? percentage,
        int discountType
    )
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(couponCode))
        {
            errors.Add(Error.Validation("Coupon code is required", "ERR_CODE_REQ", "CouponCode"));
        }

        if (discountType == 1 && fixedAmount is null)
        {
            errors.Add(Error.Validation("Fixed amount is required", "ERR_AMT_REQ", "FixedAmount"));
        }

        if (discountType == 1 && fixedAmount is not null && fixedAmount <= 0)
        {
            errors.Add(Error.Validation("Fixed amount must be greater than 0", "ERR_AMT_GT_0", "FixedAmount"));
        }

        if (discountType == 2 && percentage is null)
        {
            errors.Add(Error.Validation("Percentage is required", "ERR_AMT_REQ", "Percentage"));
        }

        if (discountType == 2 && percentage is not null && percentage <= 0)
        {
            errors.Add(Error.Validation("Percentage must be greater than 0", "ERR_AMT_GT_0", "Percentage"));
        }

        if (discountType < 1 || discountType > 3)
        {
            errors.Add(Error.Validation("Invalid discount type", "ERR_TYPE_INV", "DiscountType"));
        }

        return errors;
    }
}