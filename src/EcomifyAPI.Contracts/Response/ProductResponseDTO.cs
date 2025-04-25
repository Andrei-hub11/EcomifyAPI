using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Response;

public sealed record ProductResponseDTO(
Guid Id,
string Name,
string Description,
decimal Price,
string CurrencyCode,
int Stock,
string ImageUrl,
ProductStatusDTO Status,
IReadOnlyList<CategoryResponseDTO> Categories);