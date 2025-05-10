using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.DapperModels;

public class OrderMapping
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatusDTO Status { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalWithDiscount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public ShippingAddressMapping ShippingAddress { get; set; } = new();
    public BillingAddressMapping BillingAddress { get; set; } = new();
    public List<OrderItemMapping> Items { get; set; } = [];

    // Properties to map the columns from the SQL query
    public string ShippingStreet { get; set; } = string.Empty;
    public int ShippingNumber { get; set; } = 0;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingZipCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingComplement { get; set; } = string.Empty;

    public string BillingStreet { get; set; } = string.Empty;
    public int BillingNumber { get; set; } = 0;
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingZipCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;
    public string BillingComplement { get; set; } = string.Empty;

    // Property to map the items column
    public string ItemsJson { get; set; } = string.Empty;
}