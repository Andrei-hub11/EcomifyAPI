using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.Request;

public sealed record OrderFilterRequestDTO(
    Guid? Id = null,
    string? UserId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    OrderStatusDTO? Status = null,
    decimal? MinAmount = null,
    decimal? MaxAmount = null,
    int Page = 1,
    int PageSize = 10,
    string SortBy = "OrderDate",
    bool SortAscending = false
);