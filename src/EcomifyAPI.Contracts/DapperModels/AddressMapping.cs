namespace EcomifyAPI.Contracts.DapperModels;

public class ShippingAddressMapping
{
    public string ShippingStreet { get; set; } = string.Empty;
    public int ShippingNumber { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingZipCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public string ShippingComplement { get; set; } = string.Empty;
}

public class BillingAddressMapping
{
    public string BillingStreet { get; set; } = string.Empty;
    public int BillingNumber { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingZipCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = string.Empty;
    public string BillingComplement { get; set; } = string.Empty;
}