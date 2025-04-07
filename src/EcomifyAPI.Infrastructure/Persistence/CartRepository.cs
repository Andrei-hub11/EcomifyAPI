using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Infrastructure.Persistence;

public class CartRepository : ICartRepository
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

    public async Task<CartMapping?> GetCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT c.id, c.user_keycloak_id AS userId, c.total_amount AS totalAmount, c.currency_code AS currencyCode, 
                   c.created_at AS createdAt, ci.id AS itemId, ci.product_id AS productId, ci.quantity, ci.unit_price AS unitPrice, 
                   ci.total_price AS itemTotalPrice, ci.currency_code AS itemCurrencyCode
            FROM carts c
            LEFT JOIN cart_items ci ON c.id = ci.cart_id
            WHERE c.user_keycloak_id = @UserId;
        ";

        var cartDictionary = new Dictionary<Guid, CartMapping>();

        await Connection.QueryAsync<CartMapping, CartItemMapping, CartMapping>(
            new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken, transaction: Transaction),
            (cart, item) =>
            {
                if (!cartDictionary.TryGetValue(cart.Id, out var existingCart))
                {
                    existingCart = cart;
                    cartDictionary[cart.Id] = existingCart;
                }

                if (item != null)
                {
                    existingCart.Items.Add(item);
                }

                return existingCart;
            }, splitOn: "itemId");

        return cartDictionary.Values.FirstOrDefault();
    }

    public async Task<CartMapping> CreateCartAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO carts (user_keycloak_id, total_amount, currency_code)
            VALUES (@UserId, @TotalAmount, @CurrencyCode);
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                cart.UserId,
                TotalAmount = cart.TotalAmount.Amount,
                CurrencyCode = cart.TotalAmount.Code
            }, cancellationToken: cancellationToken, transaction: Transaction));

        var createdCart = await GetCartAsync(cart.UserId, cancellationToken);

        return createdCart ?? throw new InvalidOperationException("Cart was not created.");
    }

    public async Task AddItemAsync(Guid cartId, CartItem item, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO cart_items (cart_id, product_id, quantity, unit_price, total_price, currency_code)
            VALUES (@CartId, @ProductId, @Quantity, @UnitPrice, @TotalPrice, @CurrencyCode);
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                CartId = cartId,
                item.ProductId,
                item.Quantity,
                UnitPrice = item.UnitPrice.Amount,
                TotalPrice = item.TotalPrice.Amount,
                CurrencyCode = item.TotalPrice.Code
            }, cancellationToken: cancellationToken, transaction: Transaction));
    }

    public async Task UpdateItemQuantityAsync(Guid cartId, Guid productId, CartItem item, CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE cart_items
            SET quantity = @Quantity,
            total_price = @TotalPrice
            WHERE cart_id = @CartId AND product_id = @ProductId;
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                CartId = cartId,
                ProductId = productId,
                item.Quantity,
                TotalPrice = item.TotalPrice.Amount
            }, cancellationToken: cancellationToken, transaction: Transaction));
    }

    public async Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM cart_items
            WHERE cart_id = @CartId;
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query, new { CartId = cartId }, cancellationToken: cancellationToken, transaction: Transaction));
    }

    public async Task RemoveItemAsync(Guid cartId, Guid productId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM cart_items
            WHERE cart_id = @CartId AND product_id = @ProductId;
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query, new { CartId = cartId, ProductId = productId },
            cancellationToken: cancellationToken, transaction: Transaction));
    }
}