namespace EcomifyAPI.Contracts.DapperModels;

public class FilteredResponseMapping<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public long TotalCount { get; set; }

    public FilteredResponseMapping(IReadOnlyList<T> items, long totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}