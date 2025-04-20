using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.DapperModels;

public class DiscountCartMapping
{
    public Guid Id { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public DiscountTypeEnum DiscountType { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal? Percentage { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool AutoApply { get; set; }
}