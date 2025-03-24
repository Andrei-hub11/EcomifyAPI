using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.DapperModels;

public class OrderMapping
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatusEnum Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public AddressMapping ShippingAddress { get; set; } = new();
    public AddressMapping BillingAddress { get; set; } = new();
    public List<OrderItemMapping> Items { get; set; } = [];
}