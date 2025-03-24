using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Infrastructure.Persistence;

public class ProductRepository : IProductRepository
{
    private IDbConnection? _connection = null;
    private IDbTransaction? _transaction = null;

    private IDbConnection Connection =>
        _connection ?? throw new InvalidOperationException("Connection has not been initialized.");
    private IDbTransaction Transaction =>
        _transaction
        ?? throw new InvalidOperationException("Transaction has not been initialized.");

    public void Initialize(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<ProductMapping?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT * FROM products WHERE id = @Id
            ";

        var product = await Connection.QueryAsync<ProductMapping>(
            new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken, transaction: Transaction)
        );

        return product.FirstOrDefault();
    }

    public async Task<CategoryMapping?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT * FROM categories WHERE id = @Id
            ";

        var category = await Connection.QueryAsync<CategoryMapping>(
            new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken, transaction: Transaction)
        );

        return category.FirstOrDefault();
    }
    public async Task<Guid> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO products (id, name, description, price, stock, image_url, status)
            VALUES (@Id, @Name, @Description, @Price, @Stock, @ImageUrl, @Status)
            RETURNING id;
            ";

        var productId = await Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(query, new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.ImageUrl,
                product.Status,
            }, cancellationToken: cancellationToken, transaction: Transaction)
        );

        return productId;
    }

    public async Task<bool> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO categories (id, name, description)
            VALUES (@Id, @Name, @Description)
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                category.Id,
                category.Name,
                category.Description
            },
            cancellationToken: cancellationToken,
            transaction: Transaction
        ));

        return result > 0;
    }

    public async Task<bool> CreateProductCategoryAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO product_categories (product_id, category_id)
            VALUES (@ProductId, @CategoryId)
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query, new
            {
                productCategory.ProductId,
                productCategory.CategoryId
            }, cancellationToken: cancellationToken, transaction: Transaction)
        );

        return result > 0;
    }

    public async Task<bool> UpdateProductAsync(Product updatedProduct, CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE products SET name = @Name, 
            description = @Description, 
            price = @Price, 
            stock = @Stock, 
            image_url = @ImageUrl, 
            status = @Status, 
            WHERE id = @Id
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query, new
            {
                updatedProduct.Id,
                updatedProduct.Name,
                updatedProduct.Description,
                updatedProduct.Price,
                updatedProduct.Stock,
                updatedProduct.ImageUrl,
                updatedProduct.Status,
            },
            cancellationToken: cancellationToken,
            transaction: Transaction
        ));

        return result > 0;
    }

    public async Task<bool> UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE categories SET name = @Name, description = @Description WHERE id = @Id
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query, new
            {
                category.Id,
                category.Name,
                category.Description
            },
            cancellationToken: cancellationToken,
            transaction: Transaction
        ));

        return result > 0;
    }


    public async Task<bool> UpdateProductCategoriesAsync(ProductCategory productCategory, CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE product_categories SET product_id = @ProductId, category_id = @CategoryId WHERE id = @Id
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query, new
            {
                productCategory.ProductId,
                productCategory.CategoryId
            },
            cancellationToken: cancellationToken,
            transaction: Transaction
        ));

        return result > 0;
    }
}