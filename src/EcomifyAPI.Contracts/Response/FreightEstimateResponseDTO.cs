using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Contracts.Response;

public sealed record FreightEstimateResponseDTO(
    MoneyDTO ShippingCost,
    int EstimatedDeliveryDays,
    string ShippingMethod
)
{
    public DateTime EstimatedDeliveryDate => DateTime.Now.AddDays(EstimatedDeliveryDays);
}