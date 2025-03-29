namespace EcomifyAPI.Contracts.DapperModels;

public class OrderItemMapping
{
    public Guid ItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}