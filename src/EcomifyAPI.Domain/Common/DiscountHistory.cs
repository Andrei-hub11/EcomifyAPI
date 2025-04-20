using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.Domain.Common;

public sealed class DiscountHistory
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string CustomerId { get; private set; }
    public Guid DiscountId { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal? Percentage { get; private set; }
    public decimal? FixedAmount { get; private set; }
    public string? CouponCode { get; private set; }
    public DateTime AppliedAt { get; private set; }

    private DiscountHistory(
        Guid id,
        Guid orderId,
        string customerId,
        Guid discountId,
        DiscountType discountType,
        decimal discountAmount,
        decimal? percentage,
        decimal? fixedAmount,
        string? couponCode,
        DateTime appliedAt)
    {
        Id = id;
        OrderId = orderId;
        CustomerId = customerId;
        DiscountId = discountId;
        DiscountType = discountType;
        DiscountAmount = discountAmount;
        Percentage = percentage;
        FixedAmount = fixedAmount;
        CouponCode = couponCode;
        AppliedAt = appliedAt;
    }

    public static Result<DiscountHistory> Create(
        Guid orderId,
        string customerId,
        Guid discountId,
        DiscountType discountType,
        decimal discountAmount,
        decimal? percentage,
        decimal? fixedAmount,
        string? couponCode)
    {
        var errors = ValidateDiscountHistory(orderId, customerId, discountId, discountType, discountAmount, percentage, fixedAmount, couponCode);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new DiscountHistory(
            Guid.NewGuid(),
            orderId,
            customerId,
            discountId,
            discountType,
            discountAmount,
            percentage,
            fixedAmount,
            couponCode,
            DateTime.UtcNow);
    }

    public static Result<DiscountHistory> From(
        Guid id,
        Guid orderId,
        string customerId,
        Guid discountId,
        DiscountType discountType,
        decimal discountAmount,
        decimal? percentage,
        decimal? fixedAmount,
        string couponCode,
        DateTime appliedAt)
    {
        var errors = ValidateDiscountHistory(orderId, customerId, discountId, discountType, discountAmount, percentage, fixedAmount, couponCode, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new DiscountHistory(
            id,
            orderId,
            customerId,
            discountId,
            discountType,
            discountAmount,
            percentage,
            fixedAmount,
            couponCode,
            appliedAt);
    }

    private static List<ValidationError> ValidateDiscountHistory(
        Guid orderId,
        string customerId,
        Guid discountId,
        DiscountType discountType,
        decimal discountAmount,
        decimal? percentage,
        decimal? fixedAmount,
        string? couponCode,
        Guid? id = null)
    {
        var errors = new List<ValidationError>();

        // Validate the discount type
        if (!Enum.IsDefined(typeof(DiscountType), discountType))
        {
            errors.Add(Error.Validation("Invalid discount type", "ERR_DISCOUNT_TYPE_INVALID", "DiscountType"));
            return errors;
        }

        if (id is not null && id == Guid.Empty)
        {
            errors.Add(Error.Validation("Id is required", "ERR_ID_REQUIRED", "Id"));
        }

        if (orderId == Guid.Empty)
        {
            errors.Add(Error.Validation("OrderId is required", "ERR_ORDER_ID_REQUIRED", "OrderId"));
        }

        if (string.IsNullOrWhiteSpace(customerId))
        {
            errors.Add(Error.Validation("CustomerId is required", "ERR_CUSTOMER_ID_REQUIRED", "CustomerId"));
        }

        if (discountId == Guid.Empty)
        {
            errors.Add(Error.Validation("DiscountId is required", "ERR_DISCOUNT_ID_REQUIRED", "DiscountId"));
        }

        if (discountAmount < 0)
        {
            errors.Add(Error.Validation("Discount amount must be greater than or equal to 0", "ERR_DISCOUNT_AMOUNT_NEGATIVE", "DiscountAmount"));
        }

        if (discountType == DiscountType.Coupon && string.IsNullOrWhiteSpace(couponCode))
        {
            errors.Add(Error.Validation("Code is required", "ERR_CODE_REQ", "code"));
        }

        // Validate based on discount type constraints
        switch (discountType)
        {
            case DiscountType.Fixed:
                ValidateFixedDiscount(fixedAmount, percentage, errors);
                break;

            case DiscountType.Percentage:
                ValidatePercentageDiscount(fixedAmount, percentage, errors);
                break;

            case DiscountType.Coupon:
                ValidateCouponDiscount(fixedAmount, percentage, errors);
                break;
        }

        return errors;
    }

    private static void ValidateFixedDiscount(decimal? fixedAmount, decimal? percentage, List<ValidationError> errors)
    {
        if (fixedAmount is null or <= 0)
        {
            errors.Add(Error.Validation("Amount must be greater than zero", "ERR_AMT_GT_0", "fixedAmount"));
        }

        if (percentage is not null)
        {
            errors.Add(Error.Validation("Percentage cannot be provided for fixed discount type", "ERR_PERC_INV", "percentage"));
        }
    }

    private static void ValidatePercentageDiscount(decimal? fixedAmount, decimal? percentage, List<ValidationError> errors)
    {
        if (percentage is null or <= 0 or > 100)
        {
            errors.Add(Error.Validation("Percentage must be between 0 and 100", "ERR_PERC_INV", "percentage"));
        }

        if (fixedAmount is not null)
        {
            errors.Add(Error.Validation("Fixed amount cannot be provided for percentage discount type", "ERR_AMT_INV", "fixedAmount"));
        }
    }

    private static void ValidateCouponDiscount(decimal? fixedAmount, decimal? percentage, List<ValidationError> errors)
    {
        var hasFixed = fixedAmount is not null && fixedAmount > 0;
        var hasPercentage = percentage is not null && percentage > 0 && percentage <= 100;

        if (!hasFixed && !hasPercentage)
        {
            errors.Add(Error.Validation("Either fixed amount or percentage must be provided", "ERR_AMT_OR_PERC_REQ", "fixedAmount"));
            errors.Add(Error.Validation("Either fixed amount or percentage must be provided", "ERR_AMT_OR_PERC_REQ", "percentage"));
        }

        if (fixedAmount is not null && fixedAmount <= 0)
        {
            errors.Add(Error.Validation("Amount must be greater than zero", "ERR_AMT_GT_0", "fixedAmount"));
        }

        if (percentage is not null && (percentage <= 0 || percentage > 100))
        {
            errors.Add(Error.Validation("Percentage must be between 0 and 100", "ERR_PERC_INV", "percentage"));
        }
    }
}