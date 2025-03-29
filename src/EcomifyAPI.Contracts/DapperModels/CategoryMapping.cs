namespace EcomifyAPI.Contracts.DapperModels;

public sealed class CategoryMapping
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryDescription { get; set; } = string.Empty;
}