using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsDiscounts
{
    public static DiscountHistoryResponseDTO ToResponseDTO(this DiscountHistoryMapping discountHistory)
    {
        return new DiscountHistoryResponseDTO(
            discountHistory.Id,
            discountHistory.OrderId,
            discountHistory.CustomerId,
            discountHistory.DiscountId,
            (DiscountTypeEnum)discountHistory.DiscountType,
            discountHistory.DiscountAmount,
            discountHistory.Percentage,
            discountHistory.FixedAmount,
            discountHistory.CouponCode,
            discountHistory.AppliedAt
        );
    }

    public static IReadOnlyList<DiscountHistoryResponseDTO> ToResponseDTO(this IEnumerable<DiscountHistoryMapping> discountHistories)
    {
        return [.. discountHistories.Select(d => d.ToResponseDTO())];
    }

    public static PaginatedResponseDTO<DiscountResponseDTO> ToResponseDTO(this FilteredResponseMapping<DiscountMapping> discounts,
    int pageNumber, int pageSize)
    {
        return new PaginatedResponseDTO<DiscountResponseDTO>(
            [.. discounts.Items.Select(d => d.ToResponseDTO())],
            pageSize,
            pageNumber,
            discounts.TotalCount
        );
    }

    public static DiscountResponseDTO ToResponseDTO(this DiscountMapping discount)
    {
        return new DiscountResponseDTO(
            discount.Id,
            discount.Code,
            discount.FixedAmount,
            discount.Percentage,
            discount.MaxUses,
            discount.Uses,
            discount.MinOrderAmount,
            discount.MaxUsesPerUser,
            discount.ValidFrom,
            discount.ValidTo,
            discount.AutoApply,
            discount.DiscountType,
            [.. discount.Categories.Select(c => c.ToResponseDTO())]
        );
    }

    public static DiscountResponseDTO ToResponseDTO(this Discount discount, HashSet<CategoryResponseDTO> categories)
    {
        return new DiscountResponseDTO(
            discount.Id,
            discount.Code,
            discount.FixedAmount,
            discount.Percentage,
            discount.MaxUses,
            discount.Uses,
            discount.MinOrderAmount,
            discount.MaxUsesPerUser,
            discount.ValidFrom,
            discount.ValidTo,
            discount.AutoApply,
            (DiscountTypeEnum)discount.DiscountType,
            categories
        );
    }

    public static IReadOnlyList<DiscountResponseDTO> ToResponseDTO(this IEnumerable<DiscountMapping> discounts)
    {
        return [.. discounts.Select(d => d.ToResponseDTO())];
    }
}