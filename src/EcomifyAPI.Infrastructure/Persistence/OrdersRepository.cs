using System.Data;
using System.Text;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Common.Helpers;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

using Newtonsoft.Json;

namespace EcomifyAPI.Infrastructure.Persistence;

public class OrderRepository : IOrderRepository
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

    public async Task<IEnumerable<OrderMapping>> GetAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT 
            o.id AS Id,
            o.user_keycloak_id AS UserId,
            o.currency_code AS CurrencyCode,
            o.order_date AS OrderDate,
            o.status AS Status,
            o.total_amount AS TotalAmount,
            o.discount_amount AS DiscountAmount,
            o.total_with_discount AS TotalWithDiscount,
            o.shipping_street AS ShippingStreet,
            o.shipping_number AS ShippingNumber,
            o.shipping_city AS ShippingCity,
            o.shipping_state AS ShippingState,
            o.shipping_zip_code AS ShippingZipCode,
            o.shipping_country AS ShippingCountry,
            o.shipping_complement AS ShippingComplement,
            o.billing_street AS BillingStreet,
            o.billing_number AS BillingNumber,
            o.billing_city AS BillingCity,
            o.billing_state AS BillingState,
            o.billing_zip_code AS BillingZipCode,
            o.billing_country AS BillingCountry,
            o.billing_complement AS BillingComplement,
            COALESCE(
                JSON_AGG(
                    JSON_BUILD_OBJECT(
                        'ItemId', i.id,
                        'OrderId', i.order_id,
                        'ProductId', i.product_id,
                        'ProductName', p.name,
                        'Quantity', i.quantity,
                        'UnitPrice', i.unit_price,
                        'CurrencyCode', i.currency_code,
                        'TotalPrice', i.total_price
                    )
                ) FILTER (WHERE i.id IS NOT NULL),
                '[]'::json
            ) AS ItemsJson
        FROM orders o
        LEFT JOIN order_items i ON o.id = i.order_id
        LEFT JOIN products p ON i.product_id = p.id
        GROUP BY 
            o.id,
            o.user_keycloak_id,
            o.currency_code,
            o.order_date,
            o.status,
            o.total_amount,
            o.discount_amount,
            o.total_with_discount,
            o.shipping_street,
            o.shipping_city,
            o.shipping_state,
            o.shipping_zip_code,
            o.shipping_country,
            o.billing_street,
            o.billing_city,
            o.billing_state,
            o.billing_zip_code,
            o.billing_country
    ";

        var orders = await Connection.QueryAsync<OrderMapping>(
            new CommandDefinition(query, cancellationToken: cancellationToken, transaction: Transaction)
        );

        foreach (var order in orders)
        {
            var items = JsonConvert.DeserializeObject<List<OrderItemMapping>>(order.ItemsJson);

            ThrowHelper.ThrowIfNull(items, "Items");

            order.Items = items;

            order.ShippingAddress = new ShippingAddressMapping
            {
                ShippingStreet = order.ShippingStreet,
                ShippingNumber = order.ShippingNumber,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingZipCode = order.ShippingZipCode,
                ShippingCountry = order.ShippingCountry
            };

            order.BillingAddress = new BillingAddressMapping
            {
                BillingStreet = order.BillingStreet,
                BillingNumber = order.BillingNumber,
                BillingCity = order.BillingCity,
                BillingState = order.BillingState,
                BillingZipCode = order.BillingZipCode,
                BillingCountry = order.BillingCountry
            };
        }

        return orders;
    }

    public async Task<OrderMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string query = @"
                    SELECT 
            o.id AS Id,
            o.user_keycloak_id AS UserId,
            o.currency_code AS CurrencyCode,
            o.order_date AS OrderDate,
            o.status AS Status,
            o.total_amount AS TotalAmount,
            o.discount_amount AS DiscountAmount,
            o.total_with_discount AS TotalWithDiscount,
            o.shipping_street AS ShippingStreet,
            o.shipping_number AS ShippingNumber,
            o.shipping_city AS ShippingCity,
            o.shipping_state AS ShippingState,
            o.shipping_zip_code AS ShippingZipCode,
            o.shipping_country AS ShippingCountry,
            o.shipping_complement AS ShippingComplement,
            o.billing_street AS BillingStreet,
            o.billing_number AS BillingNumber,
            o.billing_city AS BillingCity,
            o.billing_state AS BillingState,
            o.billing_zip_code AS BillingZipCode,
            o.billing_country AS BillingCountry,
            o.billing_complement AS BillingComplement,
            COALESCE(
                JSON_AGG(
                    JSON_BUILD_OBJECT(
                        'ItemId', i.id,
                        'OrderId', i.order_id,
                        'ProductId', i.product_id,
                        'ProductName', p.name,
                        'Quantity', i.quantity,
                        'UnitPrice', i.unit_price,
                        'CurrencyCode', i.currency_code,
                        'TotalPrice', i.total_price
                    )
                ) FILTER (WHERE i.id IS NOT NULL),
                '[]'::json
            ) AS ItemsJson
        FROM orders o
        LEFT JOIN order_items i ON o.id = i.order_id
        LEFT JOIN products p ON i.product_id = p.id
        WHERE o.id = @Id
        GROUP BY 
            o.id,
            o.user_keycloak_id,
            o.currency_code,
            o.order_date,
            o.status,
            o.total_amount,
            o.discount_amount,
            o.total_with_discount,
            o.shipping_street,
            o.shipping_city,
            o.shipping_state,
            o.shipping_zip_code,
            o.shipping_country,
            o.billing_street,
            o.billing_city,
            o.billing_state,
            o.billing_zip_code,
            o.billing_country
            ";

        var orders = await Connection.QueryAsync<OrderMapping>(
           new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken, transaction: Transaction)
       );

        foreach (var order in orders)
        {
            var items = JsonConvert.DeserializeObject<List<OrderItemMapping>>(order.ItemsJson);

            ThrowHelper.ThrowIfNull(items, "Items");

            order.Items = items;

            order.ShippingAddress = new ShippingAddressMapping
            {
                ShippingStreet = order.ShippingStreet,
                ShippingNumber = order.ShippingNumber,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingZipCode = order.ShippingZipCode,
                ShippingCountry = order.ShippingCountry,
                ShippingComplement = order.ShippingComplement
            };

            order.BillingAddress = new BillingAddressMapping
            {
                BillingStreet = order.BillingStreet,
                BillingNumber = order.BillingNumber,
                BillingCity = order.BillingCity,
                BillingState = order.BillingState,
                BillingZipCode = order.BillingZipCode,
                BillingCountry = order.BillingCountry,
                BillingComplement = order.BillingComplement
            };
        }

        return orders.FirstOrDefault();
    }

    public async Task<Guid> CreateAsync(Order order, string currencyCode, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO orders (user_keycloak_id, total_amount, discount_amount, total_with_discount, currency_code, 
            order_date, status, created_at, completed_at, 
            shipping_street, shipping_number, shipping_city, shipping_state, shipping_zip_code, 
            shipping_country, shipping_complement, billing_street, billing_number, billing_city, billing_state, 
            billing_zip_code, billing_country, billing_complement)
            VALUES (@UserId, @TotalAmount, @DiscountAmount, @TotalWithDiscount, @CurrencyCode, 
            @OrderDate, @Status, @CreatedAt, @CompletedAt, 
            @ShippingStreet, @ShippingNumber, @ShippingCity, @ShippingState, @ShippingZipCode, 
            @ShippingCountry, @ShippingComplement, @BillingStreet, @BillingNumber, @BillingCity, @BillingState, 
            @BillingZipCode, @BillingCountry, @BillingComplement)
            RETURNING id
            ";

        var result = await Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                query,
                new
                {
                    order.UserId,
                    TotalAmount = order.TotalAmount.Amount,
                    order.DiscountAmount,
                    TotalWithDiscount = order.TotalWithDiscount.Amount,
                    CurrencyCode = currencyCode,
                    order.OrderDate,
                    order.Status,
                    order.CreatedAt,
                    order.CompletedAt,
                    ShippingStreet = order.ShippingAddress.Street,
                    ShippingNumber = order.ShippingAddress.Number,
                    ShippingCity = order.ShippingAddress.City,
                    ShippingState = order.ShippingAddress.State,
                    ShippingZipCode = order.ShippingAddress.ZipCode,
                    ShippingCountry = order.ShippingAddress.Country,
                    ShippingComplement = order.ShippingAddress.Complement,
                    BillingStreet = order.BillingAddress.Street,
                    BillingNumber = order.BillingAddress.Number,
                    BillingCity = order.BillingAddress.City,
                    BillingState = order.BillingAddress.State,
                    BillingZipCode = order.BillingAddress.ZipCode,
                    BillingCountry = order.BillingAddress.Country,
                    BillingComplement = order.BillingAddress.Complement,
                },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );

        return result;
    }

    public async Task<bool> CreateOrderItemAsync(OrderItem orderItem, Guid orderId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO order_items (order_id, product_id, quantity, unit_price, total_price, currency_code)
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice, @CurrencyCode)  
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                orderId,
                orderItem.ProductId,
                orderItem.Quantity,
                UnitPrice = orderItem.UnitPrice.Amount,
                TotalPrice = orderItem.TotalPrice.Amount,
                CurrencyCode = orderItem.TotalPrice.Code,
            },
            cancellationToken: cancellationToken,
            transaction: Transaction)
        );

        return result > 0;
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        const string query = @"
        UPDATE orders
        SET status = @Status
        WHERE id = @Id";

        await Connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new
                {
                    order.Id,
                    order.Status,
                },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );
    }

    public async Task<bool> DeleteAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM orders WHERE id = @Id
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query, new { Id = orderId }, cancellationToken: cancellationToken, transaction: Transaction)
        );

        return result > 0;
    }

    public async Task<OrderMapping?> GetLatestOrderByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string query = @"
        SELECT 
            o.id AS Id,
            o.user_keycloak_id AS UserId,
            o.currency_code AS CurrencyCode,
            o.order_date AS OrderDate,
            o.status AS Status,
            o.total_amount AS TotalAmount,
            o.discount_amount AS DiscountAmount,
            o.total_with_discount AS TotalWithDiscount,
            o.shipping_street AS ShippingStreet,
            o.shipping_number AS ShippingNumber,
            o.shipping_city AS ShippingCity,
            o.shipping_state AS ShippingState,
            o.shipping_zip_code AS ShippingZipCode,
            o.shipping_country AS ShippingCountry,
            o.shipping_complement AS ShippingComplement,
            o.billing_street AS BillingStreet,
            o.billing_number AS BillingNumber,
            o.billing_city AS BillingCity,
            o.billing_state AS BillingState,
            o.billing_zip_code AS BillingZipCode,
            o.billing_country AS BillingCountry,
            o.billing_complement AS BillingComplement,
            COALESCE(
                JSON_AGG(
                    JSON_BUILD_OBJECT(
                        'ItemId', i.id,
                        'OrderId', i.order_id,
                        'ProductId', i.product_id,
                        'ProductName', p.name,
                        'Quantity', i.quantity,
                        'UnitPrice', i.unit_price,
                        'CurrencyCode', i.currency_code,
                        'TotalPrice', i.total_price
                    )
                ) FILTER (WHERE i.id IS NOT NULL),
                '[]'::json
            ) AS ItemsJson
        FROM orders o
        LEFT JOIN order_items i ON o.id = i.order_id
        LEFT JOIN products p ON i.product_id = p.id
        WHERE o.user_keycloak_id = @UserId
        GROUP BY 
            o.id,
            o.user_keycloak_id,
            o.currency_code,
            o.order_date,
            o.status,
            o.total_amount,
            o.discount_amount,
            o.total_with_discount,
            o.shipping_street,
            o.shipping_city,
            o.shipping_state,
            o.shipping_zip_code,
            o.shipping_country,
            o.billing_street,
            o.billing_city,
            o.billing_state,
            o.billing_zip_code,
            o.billing_country
        ORDER BY o.created_at DESC
        LIMIT 1";

        var orders = await Connection.QueryAsync<OrderMapping>(
            new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken, transaction: Transaction)
        );

        foreach (var order in orders)
        {
            var items = JsonConvert.DeserializeObject<List<OrderItemMapping>>(order.ItemsJson);

            ThrowHelper.ThrowIfNull(items, "Items");

            order.Items = items;

            order.ShippingAddress = new ShippingAddressMapping
            {
                ShippingStreet = order.ShippingStreet,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingZipCode = order.ShippingZipCode,
                ShippingCountry = order.ShippingCountry,
                ShippingComplement = order.ShippingComplement
            };

            order.BillingAddress = new BillingAddressMapping
            {
                BillingStreet = order.BillingStreet,
                BillingCity = order.BillingCity,
                BillingState = order.BillingState,
                BillingZipCode = order.BillingZipCode,
                BillingCountry = order.BillingCountry,
                BillingComplement = order.BillingComplement
            };
        }

        return orders.FirstOrDefault();
    }

    public async Task<(IEnumerable<OrderMapping> Orders, int TotalCount)> GetFilteredAsync(
        OrderFilterRequestDTO filter,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = new StringBuilder(@"
            WITH filtered_orders AS (
                SELECT 
                    o.id,
                    o.user_keycloak_id,
                    o.currency_code,
                    o.order_date,
                    o.status,
                    o.total_amount,
                    o.discount_amount,
                    o.total_with_discount,
                    o.shipping_street,
                    o.shipping_number,
                    o.shipping_city,
                    o.shipping_state,
                    o.shipping_zip_code,
                    o.shipping_country,
                    o.shipping_complement,
                    o.billing_street,
                    o.billing_number,
                    o.billing_city,
                    o.billing_state,
                    o.billing_zip_code,
                    o.billing_country,
                    o.billing_complement
                FROM orders o
                WHERE 1=1");

        var parameters = new DynamicParameters();

        if (filter.Id.HasValue)
        {
            sql.Append(" AND o.id = @Id");
            parameters.Add("Id", filter.Id.Value);
        }

        if (!string.IsNullOrEmpty(filter.UserId))
        {
            sql.Append(" AND o.user_keycloak_id = @UserId");
            parameters.Add("UserId", filter.UserId);
        }

        if (filter.StartDate.HasValue)
        {
            sql.Append(" AND o.order_date >= @StartDate");
            parameters.Add("StartDate", filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            sql.Append(" AND o.order_date <= @EndDate");
            parameters.Add("EndDate", filter.EndDate.Value);
        }

        if (filter.Status.HasValue)
        {
            sql.Append(" AND o.status = @Status");
            parameters.Add("Status", (int)filter.Status.Value);
        }

        if (filter.MinAmount.HasValue)
        {
            sql.Append(" AND o.total_with_discount >= @MinAmount");
            parameters.Add("MinAmount", filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            sql.Append(" AND o.total_with_discount <= @MaxAmount");
            parameters.Add("MaxAmount", filter.MaxAmount.Value);
        }

        // Count total number of records
        var countSql = $"SELECT COUNT(*) FROM filtered_orders";

        // Apply sorting
        sql.Append($" ORDER BY o.{filter.SortBy} {(filter.SortAscending ? "ASC" : "DESC")}");

        // Apply pagination
        sql.Append(@"),
            order_data AS (
                SELECT
                    fo.*,
                    COALESCE(
                        JSON_AGG(
                            JSON_BUILD_OBJECT(
                                'ItemId', i.id,
                                'OrderId', i.order_id,
                                'ProductId', i.product_id,
                                'ProductName', p.name,
                                'Quantity', i.quantity,
                                'UnitPrice', i.unit_price,
                                'CurrencyCode', i.currency_code,
                                'TotalPrice', i.total_price
                            )
                        ) FILTER (WHERE i.id IS NOT NULL),
                        '[]'::json
                    ) AS ItemsJson
                FROM filtered_orders fo
                LEFT JOIN order_items i ON fo.id = i.order_id
                LEFT JOIN products p ON i.product_id = p.id
                GROUP BY 
                    fo.id,
                    fo.user_keycloak_id,
                    fo.currency_code,
                    fo.order_date,
                    fo.status,
                    fo.total_amount,
                    fo.discount_amount,
                    fo.total_with_discount,
                    fo.shipping_street,
                    fo.shipping_city,
                    fo.shipping_state,
                    fo.shipping_zip_code,
                    fo.shipping_country,
                    fo.billing_street,
                    fo.billing_city,
                    fo.billing_state,
                    fo.billing_zip_code,
                    fo.billing_country
            )
            SELECT * FROM order_data
            LIMIT @PageSize OFFSET @Offset");

        parameters.Add("PageSize", filter.PageSize);
        parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);

        using var multi = await Connection.QueryMultipleAsync(
            new CommandDefinition(
                $"{sql}; {countSql}",
                parameters,
                transaction: Transaction,
                cancellationToken: cancellationToken
            )
        );

        var orders = await multi.ReadAsync<OrderMapping>();
        var totalCount = await multi.ReadSingleAsync<int>();

        foreach (var order in orders)
        {
            var items = JsonConvert.DeserializeObject<List<OrderItemMapping>>(order.ItemsJson);

            ThrowHelper.ThrowIfNull(items, "Items");

            order.Items = items;

            order.ShippingAddress = new ShippingAddressMapping
            {
                ShippingStreet = order.ShippingStreet,
                ShippingNumber = order.ShippingNumber,
                ShippingCity = order.ShippingCity,
                ShippingState = order.ShippingState,
                ShippingZipCode = order.ShippingZipCode,
                ShippingCountry = order.ShippingCountry,
                ShippingComplement = order.ShippingComplement
            };

            order.BillingAddress = new BillingAddressMapping
            {
                BillingStreet = order.BillingStreet,
                BillingNumber = order.BillingNumber,
                BillingCity = order.BillingCity,
                BillingState = order.BillingState,
                BillingZipCode = order.BillingZipCode,
                BillingCountry = order.BillingCountry,
                BillingComplement = order.BillingComplement
            };
        }

        return (orders, totalCount);
    }
}