using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record UpdateProductRequestDTO(
Guid Id,
string Name,
string Description,
decimal Price,
int Stock,
string ImageUrl,
ProductStatusEnum Status,
IReadOnlyList<ProductCategoryRequestDTO> Categories);