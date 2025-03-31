using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record UpdateProductRequestDTO(
Guid Id,
string Name,
string Description,
decimal Price,
string CurrencyCode,
int Stock,
string ImageUrl,
ProductStatusDTO Status,
IReadOnlyList<ProductCategoryRequestDTO> Categories);