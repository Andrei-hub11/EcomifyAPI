namespace EcomifyAPI.Contracts.DapperModels;

public class DiscountHistoryMapping
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public Guid DiscountId { get; set; }
    public int DiscountType { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? FixedAmount { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
}