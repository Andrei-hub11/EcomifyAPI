namespace EcomifyAPI.Contracts.Response;

public sealed record PaginatedResponseDTO<T>(
    IReadOnlyList<T> Items,
    int PageSize,
    int PageNumber,
    long TotalCount
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}