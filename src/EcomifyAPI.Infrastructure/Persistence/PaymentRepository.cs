using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

using Newtonsoft.Json;

namespace EcomifyAPI.Infrastructure.Persistence;

public sealed class PaymentRepository : IPaymentRepository
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

    public async Task<FilteredResponseMapping<PaymentRecordMapping>> GetAsync(PaymentFilterRequestDTO request,
    CancellationToken cancellationToken = default)
    {
        var parameters = new DynamicParameters();
        var whereConditions = new List<string>();

        // Base query
        var query = @"
        SELECT 
            p.id AS payment_id, p.order_id, p.amount, p.currency_code, p.payment_method as paymentMethod, 
            p.transaction_id as transactionId, p.processed_at as processedAt, p.status, 
            p.gateway_response as gatewayResponse, 
            p.cc_last_four_digits as ccLastFourDigits, p.cc_brand as ccBrand, 
            p.paypal_email as paypalEmail, p.paypal_payer_id as paypalPayerId,
            o.user_keycloak_id as customerId,
            COALESCE(jsonb_agg(
                jsonb_build_object(
                    'Id', h.id,
                    'PaymentId', h.payment_id,
                    'Status', h.status,
                    'Timestamp', h.timestamp,
                    'Reference', h.reference
                )
            ) FILTER (WHERE h.id IS NOT NULL), '[]') AS status_history,
            COUNT(*) OVER() AS totalCount
        FROM payment_records p
        INNER JOIN orders o ON p.order_id = o.id
        LEFT JOIN payment_status_history h ON p.id = h.payment_id
    ";

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            whereConditions.Add("p.customer_id = @CustomerId");
            parameters.Add("CustomerId", request.CustomerId);
        }

        if (request.Amount.HasValue)
        {
            whereConditions.Add("p.amount = @Amount");
            parameters.Add("Amount", request.Amount.Value);
        }

        if (request.Status.HasValue)
        {
            whereConditions.Add("p.status = @Status");
            parameters.Add("Status", request.Status.Value);
        }

        if (request.PaymentMethod.HasValue)
        {
            whereConditions.Add("p.payment_method = @PaymentMethod");
            parameters.Add("PaymentMethod", request.PaymentMethod.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentReference))
        {
            whereConditions.Add("p.payment_reference = @PaymentReference");
            parameters.Add("PaymentReference", request.PaymentReference);
        }

        if (request.StartDate.HasValue)
        {
            whereConditions.Add("p.processed_at >= @StartDate");
            parameters.Add("StartDate", request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            whereConditions.Add("p.processed_at <= @EndDate");
            parameters.Add("EndDate", request.EndDate.Value);
        }

        if (whereConditions.Count > 0)
        {
            var whereClause = " WHERE " + string.Join(" AND ", whereConditions);
            query += whereClause;
        }

        query += " GROUP BY p.id, o.user_keycloak_id ORDER BY p.processed_at DESC";

        query += " LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.PageNumber - 1) * request.PageSize);

        long totalCount = 0;

        var payments = await Connection.QueryAsync<PaymentRecordMapping, string, long, PaymentRecordMapping>(
            new CommandDefinition(
                query,
                parameters,
                cancellationToken: cancellationToken,
                transaction: Transaction
            ),
            (payment, historyJson, count) =>
            {
                payment.StatusHistory = string.IsNullOrEmpty(historyJson)
                    ? []
                    : JsonConvert.DeserializeObject<List<PaymentRecordHistoryMapping>>(historyJson)
                    ?? throw new InvalidOperationException("Failed to deserialize status history");

                totalCount = count;

                return payment;
            },
            splitOn: "status_history,totalCount"
        );

        return new FilteredResponseMapping<PaymentRecordMapping>([.. payments], totalCount);
    }

    public async Task<PaymentRecordMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT 
            p.id AS paymentId, p.order_id AS orderId, p.amount, p.currency_code AS currencyCode, p.payment_method as paymentMethod, 
            p.transaction_id as transactionId, p.processed_at as processedAt, p.status, 
            p.gateway_response as gatewayResponse, 
            p.cc_last_four_digits as ccLastFourDigits, p.cc_brand as ccBrand, 
            p.paypal_email as paypalEmail, p.paypal_payer_id as paypalPayerId,
            o.user_keycloak_id as customerId,
            COALESCE(jsonb_agg(
                jsonb_build_object(
                    'Id', h.id,
                    'PaymentId', h.payment_id,
                    'Status', h.status,
                    'Timestamp', h.timestamp,
                    'Reference', h.reference
                )
            ) FILTER (WHERE h.id IS NOT NULL), '[]') AS status_history
        FROM payment_records p
        INNER JOIN orders o ON p.order_id = o.id
        LEFT JOIN payment_status_history h ON p.id = h.payment_id
        WHERE p.id = @Id
        GROUP BY p.id, o.user_keycloak_id
        ORDER BY p.processed_at DESC;
    ";

        var paymentRecord = await Connection.QueryAsync<PaymentRecordMapping, string, PaymentRecordMapping>(
            new CommandDefinition(query,
            new { Id = id },
            cancellationToken: cancellationToken,
            transaction: Transaction),
            (payment, historyJson) =>
            {
                payment.StatusHistory = string.IsNullOrEmpty(historyJson)
                    ? []
                    : JsonConvert.DeserializeObject<List<PaymentRecordHistoryMapping>>(historyJson)
                    ?? throw new InvalidOperationException("Failed to deserialize status history");

                return payment;
            },
            splitOn: "status_history"
        );

        return paymentRecord.FirstOrDefault();
    }

    public async Task<PaymentRecordMapping?> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT 
            p.id AS paymentId, p.order_id AS orderId, p.amount, p.currency_code AS currencyCode, p.payment_method as paymentMethod, 
            p.transaction_id as transactionId, p.processed_at as processedAt, p.status, 
            p.gateway_response as gatewayResponse, 
            p.cc_last_four_digits as ccLastFourDigits, p.cc_brand as ccBrand, 
            p.paypal_email as paypalEmail, p.paypal_payer_id as paypalPayerId,
            o.user_keycloak_id as customerId,
            COALESCE(jsonb_agg(
                jsonb_build_object(
                    'Id', h.id,
                    'PaymentId', h.payment_id,
                    'Status', h.status,
                    'Timestamp', h.timestamp,
                    'Reference', h.reference
                )
            ) FILTER (WHERE h.id IS NOT NULL), '[]') AS status_history
        FROM payment_records p
        INNER JOIN orders o ON p.order_id = o.id
        LEFT JOIN payment_status_history h ON p.id = h.payment_id
        WHERE p.transaction_id = @TransactionId
        GROUP BY p.id, o.user_keycloak_id
        ORDER BY p.processed_at DESC;
    ";

        var paymentRecord = await Connection.QueryAsync<PaymentRecordMapping, string, PaymentRecordMapping>(
            new CommandDefinition(query,
            new { TransactionId = transactionId },
            cancellationToken: cancellationToken,
            transaction: Transaction),
            (payment, historyJson) =>
            {
                payment.StatusHistory = string.IsNullOrEmpty(historyJson)
                    ? []
                    : JsonConvert.DeserializeObject<List<PaymentRecordHistoryMapping>>(historyJson)
                    ?? throw new InvalidOperationException("Failed to deserialize status history");

                return payment;
            },
            splitOn: "status_history"
        );

        return paymentRecord.FirstOrDefault();
    }

    public async Task<IEnumerable<PaymentRecordMapping>> GetPaymentsByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        const string query = @"
            SELECT p.id AS paymentId, p.order_id AS orderId, p.amount, p.currency_code AS currencyCode, p.payment_method as paymentMethod, 
            p.transaction_id as transactionId, p.processed_at as processedAt, p.status, 
            p.gateway_response as gatewayResponse, 
            p.cc_last_four_digits as ccLastFourDigits, p.cc_brand as ccBrand, 
            p.paypal_email as paypalEmail, p.paypal_payer_id as paypalPayerId,
            o.user_keycloak_id as customerId,
            COALESCE(jsonb_agg(
                jsonb_build_object(
                    'Id', h.id,
                    'PaymentId', h.payment_id,
                    'Status', h.status,
                    'Timestamp', h.timestamp,
                    'Reference', h.reference
                )
            ) FILTER (WHERE h.id IS NOT NULL), '[]') AS status_history
            FROM payment_records p
            INNER JOIN orders o ON p.order_id = o.id
            LEFT JOIN payment_status_history h ON p.id = h.payment_id
            WHERE o.user_keycloak_id = @CustomerId
            GROUP BY p.id, o.user_keycloak_id
            ORDER BY p.processed_at DESC;
        ";

        var payments = await Connection.QueryAsync<PaymentRecordMapping, string, PaymentRecordMapping>(
            new CommandDefinition(query,
            new { CustomerId = customerId },
            cancellationToken: cancellationToken, transaction: Transaction),
            (payment, historyJson) =>
            {
                payment.StatusHistory = string.IsNullOrEmpty(historyJson)
                    ? []
                    : JsonConvert.DeserializeObject<List<PaymentRecordHistoryMapping>>(historyJson)
                    ?? throw new InvalidOperationException("Failed to deserialize status history");

                return payment;
            },
            splitOn: "status_history"
        );

        return [.. payments];
    }

    public async Task<Guid> CreateAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO payment_records (
                order_id, amount, currency_code, payment_method, transaction_id, 
                processed_at, status, gateway_response,
                cc_last_four_digits, cc_brand, paypal_email, paypal_payer_id
            ) VALUES (
                @OrderId, @Amount, @Currency, @PaymentMethod, @TransactionId, 
                @ProcessedAt, @Status, @GatewayResponse,
                @CcLastFourDigits, @CcBrand, @PaypalEmail, @PaypalPayerId
            )
            RETURNING id;
        ";

        var paymentId = await Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                query,
                new
                {
                    paymentRecord.OrderId,
                    paymentRecord.Amount.Amount,
                    Currency = paymentRecord.Amount.Code,
                    paymentRecord.PaymentMethod,
                    paymentRecord.TransactionId,
                    paymentRecord.ProcessedAt,
                    paymentRecord.Status,
                    paymentRecord.GatewayResponse,
                    CcLastFourDigits = paymentRecord.PaymentMethod
                    == PaymentMethodEnum.CreditCard ? paymentRecord.GetCreditCardDetails()?.LastFourDigits : null,
                    CcBrand = paymentRecord.PaymentMethod
                    == PaymentMethodEnum.CreditCard ? paymentRecord.GetCreditCardDetails()?.CardBrand : null,
                    PaypalEmail = paymentRecord.PaymentMethod
                    == PaymentMethodEnum.PayPal ? paymentRecord.GetPayPalDetails()?.PayPalEmail.Value : null,
                    PaypalPayerId = paymentRecord.PaymentMethod
                    == PaymentMethodEnum.PayPal ? paymentRecord.GetPayPalDetails()?.PayPalPayerId : null
                },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );

        return paymentId;
    }

    public async Task CreateStatusHistoryAsync(Guid paymentId, PaymentStatusChange statusChange, CancellationToken cancellationToken = default)
    {
        const string query = @"
            INSERT INTO payment_status_history (
                payment_id, status, timestamp, reference
            ) VALUES ( 
                @PaymentId, @Status, @Timestamp, @Reference
            )
        ";

        await Connection.ExecuteAsync(new CommandDefinition(query,
        new
        {
            PaymentId = paymentId,
            statusChange.Status,
            statusChange.Timestamp,
            statusChange.Reference
        },
        cancellationToken: cancellationToken,
        transaction: Transaction));
    }

    public async Task UpdateAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default)
    {
        const string query = @"
            UPDATE payment_records
            SET status = @Status,
                gateway_response = @GatewayResponse
            WHERE id = @Id
        ";

        await Connection.ExecuteAsync(new CommandDefinition(
            query,
            new
            {
                Id = paymentRecord.PaymentId,
                paymentRecord.Status,
                paymentRecord.GatewayResponse
            },
            cancellationToken: cancellationToken,
            transaction: Transaction
        ));
    }
}