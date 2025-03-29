using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Common.Helpers;
using EcomifyAPI.Contracts.DapperModels;
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

    public async Task<IEnumerable<OrderMapping>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT 
            o.id AS Id,
            o.user_keycloak_id AS UserId,
            o.currency_code AS CurrencyCode,
            o.order_date AS OrderDate,
            o.status AS Status,
            o.total_amount AS TotalAmount,
            o.shipping_street AS ShippingStreet,
            o.shipping_city AS ShippingCity,
            o.shipping_state AS ShippingState,
            o.shipping_zip_code AS ShippingZipCode,
            o.shipping_country AS ShippingCountry,
            o.shipping_complement AS ShippingComplement,
            o.billing_street AS BillingStreet,
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
        GROUP BY 
            o.id,
            o.user_keycloak_id,
            o.currency_code,
            o.order_date,
            o.status,
            o.total_amount,
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
            new CommandDefinition(query, transaction: Transaction, cancellationToken: cancellationToken)
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
                ShippingCountry = order.ShippingCountry
            };

            order.BillingAddress = new BillingAddressMapping
            {
                BillingStreet = order.BillingStreet,
                BillingCity = order.BillingCity,
                BillingState = order.BillingState,
                BillingZipCode = order.BillingZipCode,
                BillingCountry = order.BillingCountry
            };
        }

        return orders;
    }

    public async Task<OrderMapping?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
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
            o.shipping_street AS ShippingStreet,
            o.shipping_city AS ShippingCity,
            o.shipping_state AS ShippingState,
            o.shipping_zip_code AS ShippingZipCode,
            o.shipping_country AS ShippingCountry,
            o.shipping_complement AS ShippingComplement,
            o.billing_street AS BillingStreet,
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
        WHERE o.id = @Id
        GROUP BY 
            o.id,
            o.user_keycloak_id,
            o.currency_code,
            o.order_date,
            o.status,
            o.total_amount,
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
           new CommandDefinition(query, new { Id = id }, transaction: Transaction, cancellationToken: cancellationToken)
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

    public async Task<Guid> CreateOrderAsync(Order order, string currencyCode, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO orders (id, user_keycloak_id, total_amount, currency_code, 
            order_date, status, created_at, completed_at, 
            shipping_street, shipping_city, shipping_state, shipping_zip_code, 
            shipping_country, shipping_complement, billing_street, billing_city, billing_state, 
            billing_zip_code, billing_country, billing_complement)
            VALUES (@Id, @UserId, @TotalAmount, @CurrencyCode, 
            @OrderDate, @Status, @CreatedAt, @CompletedAt, 
            @ShippingStreet, @ShippingCity, @ShippingState, @ShippingZipCode, 
            @ShippingCountry, @ShippingComplement, @BillingStreet, @BillingCity, 
            @BillingState, @BillingZipCode, @BillingCountry, @BillingComplement)
            RETURNING id
            ";

        var result = await Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                query,
                new
                {
                    order.Id,
                    order.UserId,
                    TotalAmount = order.TotalAmount.Amount,
                    CurrencyCode = currencyCode,
                    order.OrderDate,
                    order.Status,
                    order.CreatedAt,
                    order.CompletedAt,
                    ShippingStreet = order.ShippingAddress.Street,
                    ShippingCity = order.ShippingAddress.City,
                    ShippingState = order.ShippingAddress.State,
                    ShippingZipCode = order.ShippingAddress.ZipCode,
                    ShippingCountry = order.ShippingAddress.Country,
                    ShippingComplement = order.ShippingAddress.Complement,
                    BillingStreet = order.BillingAddress.Street,
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

    public async Task<bool> DeleteOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM orders WHERE id = @Id
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query, new { Id = orderId }, transaction: Transaction, cancellationToken: cancellationToken)
        );

        return result > 0;
    }
}