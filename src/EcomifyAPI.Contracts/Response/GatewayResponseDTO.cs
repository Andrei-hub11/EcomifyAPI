namespace EcomifyAPI.Contracts.Response;

public sealed record GatewayResponseDTO(
    Guid TransactionId,
    string Reference,
    bool IsSuccess
);