using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IProductRepository : IRepository
{
    Task<ProductMapping?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryMapping?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> CreateProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(Product request, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductCategoriesAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
}