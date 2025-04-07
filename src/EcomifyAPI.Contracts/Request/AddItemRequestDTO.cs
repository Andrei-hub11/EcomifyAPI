namespace EcomifyAPI.Contracts.Request;

public sealed record AddItemRequestDTO(Guid ProductId, int Quantity);