using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.Infrastructure.Persistence;

public class DiscountRepository : IDiscountRepository
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

    public async Task<FilteredResponseMapping<DiscountMapping>> GetAllDiscountsAsync(
        DiscountFilterRequestDTO filter,
        CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        var whereConditions = new List<string>();
        var countQuery = "SELECT COUNT(*) FROM discounts d";

        // Base query
        var query = @"
        SELECT d.id, d.code, d.discount_type as DiscountType, d.fixed_amount as FixedAmount, 
        d.percentage as Percentage, d.max_uses as MaxUses, d.uses, 
        d.min_order_amount as MinOrderAmount, d.max_uses_per_user as MaxUsesPerUser, 
        d.valid_from as ValidFrom, d.valid_to as ValidTo, d.is_active as IsActive, 
        d.auto_apply as AutoApply, d.created_at as CreatedAt";

        // If CategoryId is provided or AutoApply is true, include categories
        var includeCategories = filter.CategoryId.HasValue || filter.AutoApply == true;
        if (includeCategories)
        {
            query += @",
            c.id as CategoryId, c.name as CategoryName, c.description as CategoryDescription";
        }

        query += " FROM discounts d";

        if (includeCategories)
        {
            query += @" 
            LEFT JOIN discount_categories dc ON d.id = dc.discount_id
            LEFT JOIN categories c ON dc.category_id = c.id";

            countQuery += @" 
            LEFT JOIN discount_categories dc ON d.id = dc.discount_id
            LEFT JOIN categories c ON dc.category_id = c.id";
        }

        if (filter.Code != null)
        {
            whereConditions.Add("d.code ILIKE @Code");
            parameters.Add("Code", $"%{filter.Code}%");
        }

        if (filter.CustomerId != null)
        {
            whereConditions.Add("d.customer_id = @CustomerId");
            parameters.Add("CustomerId", filter.CustomerId);
        }

        if (filter.CategoryId.HasValue)
        {
            whereConditions.Add("c.id = @CategoryId");
            parameters.Add("CategoryId", filter.CategoryId.Value);
        }

        if (filter.Status.HasValue)
        {
            whereConditions.Add("d.is_active = @Status");
            parameters.Add("Status", filter.Status.Value);
        }

        if (filter.Type.HasValue)
        {
            whereConditions.Add("d.discount_type = @Type");
            parameters.Add("Type", (int)filter.Type.Value);
        }

        if (filter.MinOrderAmount.HasValue)
        {
            whereConditions.Add("d.min_order_amount >= @MinOrderAmount");
            parameters.Add("MinOrderAmount", filter.MinOrderAmount.Value);
        }

        if (filter.MaxOrderAmount.HasValue)
        {
            whereConditions.Add("d.min_order_amount <= @MaxOrderAmount");
            parameters.Add("MaxOrderAmount", filter.MaxOrderAmount.Value);
        }

        if (filter.IsActive.HasValue)
        {
            whereConditions.Add("d.is_active = @IsActive");
            parameters.Add("IsActive", filter.IsActive.Value);
        }

        if (filter.AutoApply.HasValue)
        {
            whereConditions.Add("d.auto_apply = @AutoApply");
            parameters.Add("AutoApply", filter.AutoApply.Value);
        }

        // Build the WHERE clause
        if (whereConditions.Count > 0)
        {
            var whereClause = " WHERE " + string.Join(" AND ", whereConditions);
            query += whereClause;
            countQuery += whereClause;
        }

        // Add pagination
        query += " ORDER BY d.created_at DESC";
        query += " LIMIT @PageSize OFFSET @Offset";
        parameters.Add("PageSize", filter.PageSize);
        parameters.Add("Offset", (filter.PageNumber - 1) * filter.PageSize);

        // Execute the count query
        var totalCount = await Connection.ExecuteScalarAsync<long>(
            new CommandDefinition(countQuery, parameters, cancellationToken: cancellationToken, transaction: Transaction));

        // Execute the main query
        var discountDictionary = new Dictionary<Guid, DiscountMapping>();

        if (!includeCategories)
        {
            var discounts = await Connection.QueryAsync<DiscountMapping>(
                new CommandDefinition(query, parameters, cancellationToken: cancellationToken, transaction: Transaction));

            foreach (var discount in discounts)
            {
                discountDictionary[discount.Id] = discount;
            }

            return new FilteredResponseMapping<DiscountMapping>([.. discountDictionary.Values], totalCount);
        }

        await Connection.QueryAsync<DiscountMapping, CategoryMapping, DiscountMapping>(
               new CommandDefinition(query, parameters, cancellationToken: cancellationToken, transaction: Transaction),
               (discount, category) =>
               {
                   if (!discountDictionary.TryGetValue(discount.Id, out var discountEntry))
                   {
                       discountEntry = discount;
                       discountEntry.Categories = new List<CategoryMapping>();
                       discountDictionary.Add(discount.Id, discountEntry);
                   }

                   if (category != null && category.CategoryId != Guid.Empty)
                   {
                       discountEntry.Categories.Add(category);
                   }

                   return discount;
               },
               splitOn: "CategoryId"
           );

        return new FilteredResponseMapping<DiscountMapping>([.. discountDictionary.Values], totalCount);
    }

    public async Task<DiscountMapping?> GetDiscountByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id, code, discount_type as DiscountType, fixed_amount as FixedAmount, percentage as Percentage, max_uses as MaxUses, 
            uses, min_order_amount as MinOrderAmount, max_uses_per_user as MaxUsesPerUser, valid_from as ValidFrom, 
            valid_to as ValidTo, is_active as IsActive, auto_apply as AutoApply, created_at as CreatedAt FROM discounts 
            WHERE code = @Code;
        ";

        var discount = await Connection.QueryAsync<DiscountMapping>(
            new CommandDefinition(query, new { Code = code }, cancellationToken: cancellationToken, transaction: Transaction));

        return discount.FirstOrDefault();
    }

    public async Task<DiscountMapping?> GetDiscountByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT id, code, discount_type as DiscountType, fixed_amount as FixedAmount, percentage as Percentage, max_uses as MaxUses, 
            uses, min_order_amount as MinOrderAmount, max_uses_per_user as MaxUsesPerUser, valid_from as ValidFrom, 
            valid_to as ValidTo, is_active as IsActive, auto_apply as AutoApply, created_at as CreatedAt FROM discounts WHERE id = @Id;
        ";

        var discount = await Connection.QueryAsync<DiscountMapping>(
            new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken, transaction: Transaction));

        return discount.FirstOrDefault();
    }

    public async Task<DiscountMapping?> GetDiscountForCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT
                d.id,
                d.code, 
                d.discount_type AS discountType,
                d.fixed_amount AS fixedAmount,
                d.percentage AS percentage,
                d.max_uses AS maxUses,
                d.uses AS uses,
                d.min_order_amount AS minOrderAmount,
                d.max_uses_per_user AS maxUsesPerUser,
                d.valid_from AS validFrom,
                d.valid_to AS validTo,
                d.is_active AS isActive,
                d.auto_apply AS autoApply,
                d.created_at AS createdAt,
                ad.applied_at AS appliedAt 
                FROM applied_discounts ad
                JOIN discounts d ON d.id = ad.discount_id
                WHERE ad.cart_id = @CartId
                ORDER BY ad.applied_at;
        ";

        var discount = await Connection.QueryAsync<DiscountMapping>(
            new CommandDefinition(query, new { CartId = cartId }, cancellationToken: cancellationToken, transaction: Transaction));

        return discount.FirstOrDefault();
    }

    public async Task<int> GetUserUsagesAsync(string userId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT COUNT(*) FROM discount_history WHERE customer_id = @CustomerId;
        ";

        var usages = await Connection.QueryAsync<int>(
            new CommandDefinition(query, new { CustomerId = userId }, cancellationToken: cancellationToken, transaction: Transaction));

        return usages.FirstOrDefault();
    }

    public async Task<IEnumerable<DiscountMapping>> GetRecentDiscountsByCustomerIdAsync(string customerId, DateTime startDate,
    CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT d.id, d.customer_id as customerId, d.code, d.discount_type as discountType, d.max_uses as maxUses, 
            d.percentage, d.fixed_amount as fixedAmount, d.uses, d.min_order_amount as minOrderAmount, 
            d.max_uses_per_user as maxUsesPerUser, d.valid_from as validFrom, d.valid_to as validTo, d.is_active as isActive, 
            d.auto_apply as autoApply, d.created_at as createdAt 
            FROM discounts d
            WHERE d.customer_id = @CustomerId AND d.created_at >= @StartDate;
        ";

        var discounts = await Connection.QueryAsync<DiscountMapping>(
            new CommandDefinition(query,
            new
            {
                CustomerId = customerId,
                StartDate = startDate
            },
                cancellationToken: cancellationToken,
                transaction: Transaction));

        return [.. discounts];
    }


    public async Task<IEnumerable<DiscountHistoryMapping>> GetDiscountHistoryByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT
                id AS Id,
                order_id AS OrderId,
                customer_id AS CustomerId,
                discount_id AS DiscountId,
                discount_type AS DiscountType,
                discount_amount AS DiscountAmount,
                percentage AS Percentage,
                fixed_amount AS FixedAmount,
                coupon_code AS CouponCode,
                applied_at AS AppliedAt
            FROM discount_history
            WHERE order_id = @OrderId
            ";

        var result = await Connection.QueryAsync<DiscountHistoryMapping>(
            new CommandDefinition(
                query,
                new { OrderId = orderId },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );


        return result;
    }

    public async Task<IEnumerable<DiscountMapping>> GetAvailableDiscountsForCart(Guid cartId, decimal cartTotalAmount,
    string customerId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT DISTINCT
            d.id,
            d.code,
            d.discount_type as DiscountType,
            d.percentage as Percentage,
            d.fixed_amount as FixedAmount,
            d.max_uses as MaxUses,
            d.uses,
            d.min_order_amount as MinOrderAmount,
            d.max_uses_per_user as MaxUsesPerUser,
            d.valid_from as ValidFrom,
            d.valid_to as ValidTo,
            d.is_active as IsActive,
            d.auto_apply as AutoApply,
            d.created_at as CreatedAt,
            CASE WHEN ad.cart_id IS NOT NULL THEN TRUE ELSE FALSE END as IsApplied
        FROM discounts d
        LEFT JOIN applied_discounts ad ON d.id = ad.discount_id AND ad.cart_id = '560dda48-f0bd-44a8-a34e-44fb8efaa3d7'
        WHERE d.is_active = TRUE
        AND d.valid_from <= CURRENT_TIMESTAMP
        AND d.valid_to >= CURRENT_TIMESTAMP
        AND (d.max_uses > d.uses OR d.max_uses IS NULL)
        AND (d.max_uses_per_user > (SELECT COUNT(*) FROM discount_history
                          WHERE customer_id = @CustomerId AND discount_id = d.id) 
                          OR d.max_uses_per_user IS NULL)
        AND d.min_order_amount <= @MinOrderAmount
        AND (
            /* Include already applied discounts */
            (ad.cart_id IS NOT NULL)
            OR
            /* Include auto-apply discounts that are linked to categories AND match product categories */
            (d.auto_apply = TRUE AND ad.cart_id IS NULL AND EXISTS (
                SELECT 1 FROM discount_categories dc WHERE dc.discount_id = d.id
            ) AND EXISTS (
                SELECT 1 
                FROM discount_categories dc
                JOIN product_categories pc ON dc.category_id = pc.category_id
                WHERE dc.discount_id = d.id
                AND pc.product_id = ANY(@ProductIds)
            ))
        )
        ORDER BY d.valid_from DESC;
        ";

        var discount = await Connection.QueryAsync<DiscountMapping>(
            new CommandDefinition(query,
            new
            {
                CartId = cartId,
                MinOrderAmount = cartTotalAmount,
                ProductIds = productIds.ToArray(),
                CustomerId = customerId
            },
                cancellationToken: cancellationToken,
                transaction: Transaction));

        return [.. discount];
    }

    public async Task<IEnumerable<AppliedDiscountMapping>> GetAppliedDiscountsAsync(Guid cartId,
    CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT ad.cart_id AS cartId, ad.discount_id AS discountId, 
            ad.applied_at AS appliedAt FROM applied_discounts ad WHERE ad.cart_id = @CartId;
        ";

        var appliedDiscounts = await Connection.QueryAsync<AppliedDiscountMapping>(
            new CommandDefinition(query, new { CartId = cartId }, cancellationToken: cancellationToken, transaction: Transaction));

        return [.. appliedDiscounts];
    }


    public async Task<Guid> CreateDiscountAsync(Discount discount, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO discounts (code, discount_type, fixed_amount, percentage, max_uses, uses, 
            min_order_amount, max_uses_per_user, valid_from, valid_to, is_active, auto_apply, created_at) 
            VALUES (@Code, @DiscountType, @FixedAmount, @Percentage, @MaxUses, @Uses, @MinOrderAmount, 
            @MaxUsesPerUser, @ValidFrom, @ValidTo, @IsActive, @AutoApply, @CreatedAt)
            RETURNING id;
        ";

        var result = await Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(query,
            discount,
            cancellationToken: cancellationToken, transaction: Transaction));

        return result;
    }

    public async Task CreateDiscountHistoryAsync(DiscountHistory discountHistory, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO discount_history (
                id, order_id, customer_id, discount_id, discount_type, 
                discount_amount, percentage, fixed_amount, coupon_code, applied_at)
            VALUES (
                @Id, @OrderId, @CustomerId, @DiscountId, @DiscountType, 
                @DiscountAmount, @Percentage, @FixedAmount, @CouponCode, @AppliedAt)
            ";

        var result = await Connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new
                {
                    discountHistory.Id,
                    discountHistory.OrderId,
                    discountHistory.CustomerId,
                    discountHistory.DiscountId,
                    discountHistory.DiscountType,
                    discountHistory.DiscountAmount,
                    discountHistory.Percentage,
                    discountHistory.FixedAmount,
                    discountHistory.CouponCode,
                    discountHistory.AppliedAt
                },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );
    }

    public async Task LinkDiscountToCategoryAsync(Guid discountId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO discount_categories (discount_id, category_id) VALUES (@DiscountId, @CategoryId);
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                DiscountId = discountId,
                CategoryId = categoryId
            },
                cancellationToken: cancellationToken, transaction: Transaction));
    }

    public async Task ApplyDiscountToCartAsync(Guid cartId, Guid discountId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO applied_discounts (cart_id, discount_id) VALUES (@CartId, @DiscountId);
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query,
            new
            {
                CartId = cartId,
                DiscountId = discountId
            },
                cancellationToken: cancellationToken,
                transaction: Transaction));
    }

    public async Task UpdateDiscountAsync(Discount coupon, CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE discounts SET code = @Code, discount_type = @DiscountType, fixed_amount = @FixedAmount, percentage = @Percentage, 
            max_uses = @MaxUses, uses = @Uses, min_order_amount = @MinOrderAmount, max_uses_per_user = @MaxUsesPerUser, 
            valid_from = @ValidFrom, valid_to = @ValidTo, is_active = @IsActive, auto_apply = @AutoApply, created_at = @CreatedAt WHERE id = @Id;
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query, new { Coupon = coupon }, cancellationToken: cancellationToken, transaction: Transaction));
    }

    public async Task DeleteDiscountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM discounts WHERE id = @Id;
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken, transaction: Transaction));
    }

    public async Task ClearAppliedDiscountsAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            DELETE FROM applied_discounts WHERE cart_id = @CartId;
        ";

        await Connection.ExecuteAsync(
            new CommandDefinition(query, new { CartId = cartId }, cancellationToken: cancellationToken, transaction: Transaction));
    }
}