using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record CreateDiscountRequestDTO(
    string? CouponCode,
    decimal? FixedAmount,
    decimal? Percentage,
    DateTime ValidFrom,
    DateTime ValidTo,
    int MaxUses,
    int Uses,
    decimal MinOrderAmount,
    int MaxUsesPerUser,
    bool AutoApply,
    DiscountTypeEnum DiscountType,
    HashSet<Guid> Categories
);