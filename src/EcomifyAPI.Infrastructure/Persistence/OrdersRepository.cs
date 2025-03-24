using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

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

    public async Task<OrderMapping?> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string query = @"
            SELECT 
                o.id,
                o.user_id,
                o.total_amount,
                o.currency_code,
                o.order_date,
                o.status,
                o.shipping_street,
                o.shipping_city,
                o.shipping_state,
                o.shipping_zip_code,
                o.shipping_country,
                o.billing_street,
                o.billing_city,
                o.billing_state,
                o.billing_zip_code,
                o.billing_country,
                i.id,
                i.order_id,
                i.product_id,
                i.quantity,
                i.unit_price,
                i.total_price,
                i.currency_code
            FROM orders o
            LEFT JOIN order_items i ON o.id = i.order_id
            WHERE o.id = @Id
            ";

        var result = await Connection.QueryAsync<OrderMapping, OrderItemMapping, OrderMapping>(
            query,
            (order, item) =>
            {
                order.Items.Add(item);
                return order;
            },
            splitOn: "Id, OrderId, ProductId",
            param: new { Id = id },
            transaction: Transaction
        );

        return result.FirstOrDefault();
    }

    public async Task<Guid> CreateOrderAsync(Order order, string currencyCode, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO orders (id, user_id, total_amount, currency_code, 
            order_date, status, created_at, completed_at, 
            shipping_street, shipping_city, shipping_state, shipping_zip_code, 
            shipping_country, billing_street, billing_city, billing_state, billing_zip_code, billing_country)
            VALUES (@Id, @UserId, @TotalAmount, @CurrencyCode, 
            @OrderDate, @Status, @CreatedAt, @CompletedAt, 
            @ShippingStreet, @ShippingCity, @ShippingState, @ShippingZipCode, 
            @ShippingCountry, @BillingStreet, @BillingCity, @BillingState, 
            @BillingZipCode, @BillingCountry)
            RETURNING id
            ";

        var result = await Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                query,
                new
                {
                    order.Id,
                    order.UserId,
                    order.TotalAmount,
                    currencyCode,
                    order.OrderDate,
                    order.Status,
                    order.CreatedAt,
                    order.CompletedAt,
                    ShippingStreet = order.ShippingAddress.Street,
                    ShippingCity = order.ShippingAddress.City,
                    ShippingState = order.ShippingAddress.State,
                    ShippingZipCode = order.ShippingAddress.ZipCode,
                    ShippingCountry = order.ShippingAddress.Country,
                    BillingStreet = order.BillingAddress.Street,
                    BillingCity = order.BillingAddress.City,
                    BillingState = order.BillingAddress.State,
                    BillingZipCode = order.BillingAddress.ZipCode,
                    BillingCountry = order.BillingAddress.Country,
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
            INSERT INTO order_items (order_id, product_id, quantity, unit_price, total_price)
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice)  
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                orderItem.ProductId,
                orderItem.Quantity,
                orderItem.UnitPrice,
                orderItem.TotalPrice,
                orderId
            },
            cancellationToken: cancellationToken,
            transaction: Transaction)
        );

        return result > 0;
    }
}