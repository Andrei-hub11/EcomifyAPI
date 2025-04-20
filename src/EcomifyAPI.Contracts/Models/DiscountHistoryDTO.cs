using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Models;

public record DiscountHistoryDTO(
    Guid Id,
    Guid OrderId,
    string CustomerId,
    Guid DiscountId,
    DiscountTypeEnum DiscountType,
    decimal DiscountAmount,
    decimal? Percentage,
    decimal? FixedAmount,
    string? CouponCode,
    DateTime AppliedAt
    );