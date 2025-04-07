namespace EcomifyAPI.Contracts.DapperModels;

public class CartItemMapping
{
    public Guid ItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemTotalPrice { get; set; }
    public string ItemCurrencyCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}