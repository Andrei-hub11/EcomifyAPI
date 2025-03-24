using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Response;

public sealed record ProductResponseDTO(Guid Id,
string Name,
string Description,
decimal Price,
int Stock,
string ImageUrl,
ProductStatusEnum Status,
IReadOnlyList<ProductCategoryResponseDTO> Categories);