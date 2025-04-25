using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.DapperModels;

public class DiscountMapping
{
    public Guid Id { get; set; }
    public string? Code { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal? FixedAmount { get; set; }
    public decimal? Percentage { get; set; }
    public DiscountTypeEnum DiscountType { get; set; }
    public decimal DiscountAmount { get; set; }
    public int MaxUses { get; set; }
    public int Uses { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int MaxUsesPerUser { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; }
    public bool AutoApply { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CategoryMapping> Categories { get; set; } = [];
}