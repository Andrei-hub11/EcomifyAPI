using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Response;

public sealed record DiscountResponseDTO(
    Guid Id,
    string? Code,
    decimal? FixedAmount,
    decimal? Percentage,
    int MaxUses,
    int Uses,
    decimal MinOrderAmount,
    int MaxUsesPerUser,
    DateTime ValidFrom,
    DateTime ValidTo,
    bool AutoApply,
    DiscountTypeEnum DiscountType,
    HashSet<CategoryResponseDTO> Categories
);