namespace EcomifyAPI.Contracts.Request;

public sealed record UpdateItemQuantityRequestDTO(Guid ProductId, int Quantity);