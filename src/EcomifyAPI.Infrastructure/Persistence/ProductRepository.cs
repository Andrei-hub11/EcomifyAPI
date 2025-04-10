using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Request;
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

    public async Task<IEnumerable<ProductMapping>> GetAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT 
                p.id,  
                p.name,
                p.description,
                p.price,
                p.currency_code AS currencyCode, 
                p.image_url AS imageUrl,
                p.status,
                p.stock,
                c.id AS categoryId,
                c.name AS categoryName,
                c.description AS categoryDescription,
                pc.category_id AS productCategoryId,
                pc.product_id AS productId
            FROM products p
            LEFT JOIN product_categories pc ON p.id = pc.product_id
            LEFT JOIN categories c ON pc.category_id = c.id
            ";

        var productDictionary = new Dictionary<Guid, ProductMapping>();

        var products = await Connection.QueryAsync<ProductMapping, CategoryMapping, ProductMapping>(
            new CommandDefinition(query, cancellationToken: cancellationToken, transaction: Transaction),
            (product, category) =>
            {
                if (!productDictionary.TryGetValue(product.Id, out var existingProduct))
                {
                    existingProduct = product;
                    existingProduct.ProductCategories = [];
                    existingProduct.Categories = [];
                    productDictionary[existingProduct.Id] = existingProduct;
                }

                if (category is not null && !existingProduct.Categories.Any(c => c.CategoryId == category.CategoryId))
                {
                    existingProduct.Categories.Add(category);
                }

                return existingProduct;
            },
            splitOn: "categoryId"
        );

        return [.. productDictionary.Values];
    }

    public async Task<ProductMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT 
            p.id,  
            p.name,
            p.description,
            p.price,
            p.currency_code AS currencyCode, 
            p.image_url AS imageUrl,
            p.status,
            p.stock,
            c.id AS categoryId,
            c.name AS categoryName,
            c.description AS categoryDescription,
            pc.category_id AS productCategoryId,
            pc.product_id AS productId
        FROM products p
        LEFT JOIN product_categories pc ON p.id = pc.product_id
        LEFT JOIN categories c ON pc.category_id = c.id
        WHERE p.id = @Id";

        var productDictionary = new Dictionary<Guid, ProductMapping>();

        var products = await Connection.QueryAsync<ProductMapping, CategoryMapping, ProductMapping>(
            new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken, transaction: Transaction),
            (product, category) =>
            {
                if (!productDictionary.TryGetValue(product.Id, out var existingProduct))
                {
                    existingProduct = product;
                    existingProduct.ProductCategories = [];
                    existingProduct.Categories = [];
                    productDictionary[existingProduct.Id] = existingProduct;
                }

                if (category is not null && !existingProduct.Categories.Any(c => c.CategoryId == category.CategoryId))
                {
                    existingProduct.Categories.Add(category);
                }

                return existingProduct;
            },
            splitOn: "categoryId"
        );

        return products.FirstOrDefault();
    }

    public async Task<FilteredResponseMapping<ProductMapping>> GetLowStockProductsAsync(ProductFilterRequestDTO request, CancellationToken cancellationToken = default)
    {
        string query = @"
            SELECT 
                p.id,
                p.name,
                p.description,
                p.price,
                p.currency_code AS currencyCode,
                p.image_url AS imageUrl,
                p.status,
                p.stock,
                c.id AS categoryId,
                c.name AS categoryName,
                c.description AS categoryDescription,
                COUNT(*) OVER() AS totalCount
            FROM products p
            LEFT JOIN product_categories pc ON p.id = pc.product_id
            LEFT JOIN categories c ON pc.category_id = c.id
            WHERE p.stock <= @StockThreshold
        ";

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query += " AND p.name ILIKE @Name";
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query += " AND c.name ILIKE @Category";
        }

        query += " ORDER BY p.stock ASC";

        query += " LIMIT @PageSize OFFSET @Offset";


        var parameters = new
        {
            request.StockThreshold,
            request.PageSize,
            request.PageNumber,
            Offset = (request.PageNumber - 1) * request.PageSize,
            Name = string.IsNullOrWhiteSpace(request.Name) ? null : $"%{request.Name}%",
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : $"%{request.Category}%"
        };

        var productDictionary = new Dictionary<Guid, ProductMapping>();
        long totalCount = 0;

        var products = await Connection.QueryAsync<ProductMapping, CategoryMapping, long, ProductMapping>(
        new CommandDefinition(query, parameters, cancellationToken: cancellationToken, transaction: Transaction),
        (product, category, count) =>
        {
            totalCount = count;

            if (!productDictionary.TryGetValue(product.Id, out var existingProduct))
            {
                existingProduct = product;
                existingProduct.ProductCategories = [];
                existingProduct.Categories = [];
                productDictionary[existingProduct.Id] = existingProduct;
            }

            if (category is not null && !existingProduct.Categories.Any(c => c.CategoryId == category.CategoryId))
            {
                existingProduct.Categories.Add(category);
            }

            return existingProduct;
        },
        splitOn: "categoryId,totalCount"
      );

        return new FilteredResponseMapping<ProductMapping>([.. productDictionary.Values], totalCount);
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

    public async Task<IEnumerable<CategoryMapping>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT c.id AS categoryId, c.name AS categoryName, c.description AS categoryDescription
            FROM categories c
            ";

        var categories = await Connection.QueryAsync<CategoryMapping>(
            new CommandDefinition(query, cancellationToken: cancellationToken, transaction: Transaction)
        );

        return categories;
    }

    public async Task<IEnumerable<ProductCategoryMapping>> GetProductCategoryByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT * FROM product_categories WHERE product_id = @ProductId
            ";

        var productCategory = await Connection.QueryAsync<ProductCategoryMapping>(
            new CommandDefinition(query, new { ProductId = productId }, cancellationToken: cancellationToken, transaction: Transaction)
        );

        return productCategory;
    }

    public async Task<Guid> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO products (name, description, price, currency_code, stock, image_url, status)
            VALUES (@Name, @Description, @Price, @CurrencyCode, @Stock, @ImageUrl, @Status)
            RETURNING id;
            ";

        var productId = await Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(query, new
            {
                product.Name,
                product.Description,
                Price = product.Price.Amount,
                CurrencyCode = product.Price.Code,
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
            status = @Status
            WHERE id = @Id
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query, new
            {
                updatedProduct.Id,
                updatedProduct.Name,
                updatedProduct.Description,
                Price = updatedProduct.Price.Amount,
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

    public async Task DeleteProductCategoryAsync(Guid productId, IReadOnlyList<Guid> categoryIds, CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM product_categories WHERE product_id = @ProductId 
            AND category_id NOT IN (SELECT unnest(@CategoryIds))
            ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                ProductId = productId,
                CategoryIds = categoryIds
            },
            cancellationToken: cancellationToken,
            transaction: Transaction)
        );
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM products WHERE id = @Id
            ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken, transaction: Transaction)
        );
    }
}