using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.Domain.Entities;


public sealed class Discount
{
    public Guid Id { get; private set; }
    public string? Code { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal? FixedAmount { get; private set; }
    public decimal? Percentage { get; private set; }
    public int MaxUses { get; private set; }
    public int Uses { get; private set; }
    public decimal MinOrderAmount { get; private set; }
    public int MaxUsesPerUser { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidTo { get; private set; }
    public bool IsActive { get; private set; }
    public bool AutoApply { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Discount(
        Guid id,
        string? code,
        DiscountType discountType,
        decimal? fixedAmount,
        decimal? percentage,
        int maxUses,
        int uses,
        decimal minOrderAmount,
        int maxUsesPerUser,
        DateTime validFrom,
        DateTime validTo,
        bool isActive,
        bool autoApply,
        DateTime createdAt)
    {
        Id = id;
        Code = code;
        DiscountType = discountType;
        FixedAmount = fixedAmount;
        Percentage = percentage;
        MaxUses = maxUses;
        Uses = uses;
        MinOrderAmount = minOrderAmount;
        MaxUsesPerUser = maxUsesPerUser;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = isActive;
        AutoApply = autoApply;
        CreatedAt = createdAt;
    }

    public static Result<Discount> Create(
        string? code,
        DiscountType discountType,
        decimal? fixedAmount,
        decimal? percentage,
        int maxUses,
        decimal minOrderAmount,
        int maxUsesPerUser,
        DateTime validFrom,
        DateTime validTo,
        bool autoApply)
    {
        var errors = ValidateDiscount(
            code,
            discountType,
            fixedAmount,
            percentage,
            maxUses,
            minOrderAmount,
            maxUsesPerUser,
            validFrom,
            validTo
            );

        if (errors.Count > 0)
            return Result.Fail(errors);

        return new Discount(
            id: Guid.NewGuid(),
            code: code?.Trim().ToUpperInvariant(),
            discountType: discountType,
            fixedAmount: fixedAmount,
            percentage: percentage,
            maxUses: maxUses,
            uses: 0,
            minOrderAmount: minOrderAmount,
            maxUsesPerUser: maxUsesPerUser,
            validFrom: validFrom,
            validTo: validTo,
            isActive: true,
            autoApply: autoApply,
            createdAt: DateTime.UtcNow
        );
    }

    public static Result<Discount> From(
        Guid id,
        string? code,
        DiscountType discountType,
        decimal? fixedAmount,
        decimal? percentage,
        int maxUses,
        int uses,
        decimal minOrderAmount,
        int maxUsesPerUser,
        DateTime validFrom,
        DateTime validTo,
        bool isActive,
        bool autoApply,
        DateTime createdAt)
    {
        var errors = ValidateDiscount(
            code,
            discountType,
            fixedAmount,
            percentage,
            maxUses,
            minOrderAmount,
            maxUsesPerUser,
            validFrom,
            validTo,
            isCreate: false,
            id: id);

        if (errors.Count > 0)
        {
            return Result.Fail(errors);
        }

        return new Discount(
            id,
            code,
            discountType,
            fixedAmount,
            percentage,
            maxUses,
            uses,
            minOrderAmount,
            maxUsesPerUser,
            validFrom,
            validTo,
            isActive,
            autoApply,
            createdAt
            );
    }

    public static ReadOnlyCollection<ValidationError> ValidateDiscount(
        string? code,
        DiscountType discountType,
        decimal? fixedAmount,
        decimal? percentage,
        int maxUses,
        decimal minOrderAmount,
        int maxUsesPerUser,
        DateTime validFrom,
        DateTime validTo,
        bool isCreate = true,
        Guid? id = null)
    {
        var errors = new List<ValidationError>();

        if (!Enum.IsDefined(typeof(DiscountType), discountType))
        {
            errors.Add(Error.Validation("Invalid discount type", "ERR_TYPE_INV", "discountType"));
            return errors.AsReadOnly();
        }

        if (id is not null && id == Guid.Empty)
        {
            errors.Add(Error.Validation("ID is required", "ERR_ID_REQ", "id"));
        }

        if (discountType == DiscountType.Coupon && string.IsNullOrWhiteSpace(code))
        {
            errors.Add(Error.Validation("Code is required", "ERR_CODE_REQ", "code"));
        }

        // Validate discount amount fields based on type
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

        if (maxUses < 1)
        {
            errors.Add(Error.Validation("Max uses must be at least 1", "ERR_MAXU", "maxUses"));
        }

        if (minOrderAmount < 0)
        {
            errors.Add(Error.Validation("Minimum order cannot be negative", "ERR_MIN_ORD", "minOrderAmount"));
        }

        if (maxUsesPerUser < 1)
        {
            errors.Add(Error.Validation("Max uses per user must be at least 1", "ERR_MAXU", "maxUsesPerUser"));
        }

        var from = new DateTime(validFrom.Year, validFrom.Month, validFrom.Day, validFrom.Hour, validFrom.Minute, 0);
        var to = new DateTime(validTo.Year, validTo.Month, validTo.Day, validTo.Hour, validTo.Minute, 0);

        if (from >= to)
        {
            errors.Add(Error.Validation("Invalid validity period", "ERR_DATE_INV", "validity"));
        }

        if (isCreate && validFrom < DateTime.UtcNow)
        {
            errors.Add(Error.Validation("Valid from date cannot be in the past", "ERR_DATE_INV", "validFrom"));
        }

        if (isCreate && validTo < DateTime.UtcNow)
        {
            errors.Add(Error.Validation("Valid to date cannot be in the past", "ERR_DATE_INV", "validTo"));
        }

        if (validFrom > DateTime.UtcNow + TimeSpan.FromDays(365))
        {
            errors.Add(Error.Validation("Valid from date cannot be more than 365 days in the future", "ERR_DATE_INV", "validFrom"));
        }

        if (validTo > DateTime.UtcNow + TimeSpan.FromDays(365))
        {
            errors.Add(Error.Validation("Valid to date cannot be more than 365 days in the future", "ERR_DATE_INV", "validTo"));
        }

        return errors.AsReadOnly();
    }

    public bool IsValidForUse(decimal orderAmount, int userUsages)
    {
        var now = DateTime.UtcNow;

        return IsActive &&
               now >= ValidFrom &&
               now <= ValidTo &&
               Uses < MaxUses &&
               orderAmount >= MinOrderAmount &&
               userUsages < MaxUsesPerUser;
    }

    public decimal CalculateDiscount(decimal orderAmount)
    {
        return DiscountType switch
        {
            DiscountType.Fixed => Math.Min(FixedAmount ?? 0, orderAmount),
            DiscountType.Percentage => orderAmount * (Percentage ?? 0),
            DiscountType.Coupon => FixedAmount is not null ?
            Math.Min(FixedAmount.Value, orderAmount) : orderAmount * (Percentage ?? 0),
            _ => 0
        };
    }

    public void IncrementUsage()
    {
        if (Uses >= MaxUses)
            throw new InvalidOperationException("Max usage limit reached");

        Uses++;
    }

    public void Deactivate()
    {
        IsActive = false;
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