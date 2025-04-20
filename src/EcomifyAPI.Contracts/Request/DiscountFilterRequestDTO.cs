using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record DiscountFilterRequestDTO(
    int PageSize,
    int PageNumber,
    string? Code,
    string? CustomerId,
    Guid? CategoryId,
    Guid? ProductId,
    bool? Status,
    DiscountTypeEnum? Type,
    decimal? MinOrderAmount,
    decimal? MaxOrderAmount,
    int? MinUses,
    int? MaxUses,
    int? MinUsesPerUser,
    int? MaxUsesPerUser,
    DateTime? ValidFrom,
    DateTime? ValidTo,
    bool? IsActive,
    bool? AutoApply
);