namespace EcomifyAPI.Contracts.DapperModels;

public class CartMapping
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemMapping> Items { get; set; } = [];
    public List<DiscountCartMapping> Discounts { get; set; } = [];
}