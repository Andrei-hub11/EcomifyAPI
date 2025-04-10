namespace EcomifyAPI.Contracts.Request;

public sealed record UpdateProductCategoryRequestDTO(IReadOnlyList<Guid> CategoryIds);