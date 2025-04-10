using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public record ProductFilterRequestDTO(
    int StockThreshold,
    int PageSize,
    int PageNumber,
    string? Name,
    ProductStatusDTO? Status,
    string? Category
);