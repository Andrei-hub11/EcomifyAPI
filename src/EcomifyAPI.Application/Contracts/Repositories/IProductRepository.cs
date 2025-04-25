using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IProductRepository : IRepository
{
    Task<IEnumerable<ProductMapping>> GetAsync(CancellationToken cancellationToken = default);
    Task<ProductMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductMapping>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<FilteredResponseMapping<ProductMapping>> GetLowStockProductsAsync(ProductFilterRequestDTO request,
    CancellationToken cancellationToken = default);
    Task<CategoryMapping?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryMapping>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductCategoryMapping>> GetProductCategoryByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> CreateProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductAsync(Product request, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductCategoriesAsync(ProductCategory productCategory, CancellationToken cancellationToken = default);
    Task DeleteProductCategoryAsync(Guid productId, IReadOnlyList<Guid> categoryIds, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}