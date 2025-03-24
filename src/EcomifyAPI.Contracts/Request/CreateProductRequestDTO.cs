using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record CreateProductRequestDTO(
string Name,
string Description,
decimal Price, int Stock,
string ImageUrl,
ProductStatusEnum Status,
IReadOnlySet<Guid> Categories);