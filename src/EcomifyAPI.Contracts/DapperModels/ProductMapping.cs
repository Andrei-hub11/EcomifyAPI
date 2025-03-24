using EcomifyAPI.Contracts.Enums;

namespace EcomifyAPI.Contracts.DapperModels;

public sealed class ProductMapping
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public ProductStatusEnum Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProductCategoryMapping> Categories { get; set; } = [];
}