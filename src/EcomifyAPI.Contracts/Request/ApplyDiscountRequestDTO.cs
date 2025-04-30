using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record ApplyDiscountRequestDTO(
    Guid DiscountId,
    DiscountTypeEnum DiscountType,
    decimal? Percentage,
    decimal? FixedAmount,
    string CouponCode
    );