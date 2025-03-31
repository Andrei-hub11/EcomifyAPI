using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record CreateProductRequestDTO(
string Name,
string Description,
decimal Price,
string CurrencyCode,
int Stock,
string ImageUrl,
ProductStatusDTO Status,
HashSet<Guid> Categories);