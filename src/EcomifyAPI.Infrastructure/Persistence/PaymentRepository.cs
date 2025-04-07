using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;

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

    public async Task<IEnumerable<PaymentRecordMapping>> GetAsync(CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT 
            p.id AS payment_id, p.order_id, p.amount, p.currency_code, p.payment_method, 
            p.transaction_id, p.processed_at, p.status, p.gateway_response, 
            p.cc_last_four_digits, p.cc_brand, 
            p.paypal_email, p.paypal_payer_id,
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
        LEFT JOIN payment_status_history h ON p.id = h.payment_id
        GROUP BY p.id;
    ";

        var result = await Connection.QueryAsync<PaymentRecordMapping, string, PaymentRecordMapping>(
            new CommandDefinition(
                query,
                cancellationToken: cancellationToken
            ),
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

        return [.. result];
    }

    public async Task<PaymentRecordMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string query = @"
        SELECT 
            p.id AS payment_id, p.order_id, p.amount, p.currency_code, p.payment_method, 
            p.transaction_id, p.processed_at, p.status, p.gateway_response, 
            p.cc_last_four_digits, p.cc_brand, 
            p.paypal_email, p.paypal_payer_id,
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
        LEFT JOIN payment_status_history h ON p.id = h.payment_id
        WHERE p.id = @Id
        GROUP BY p.id;
    ";

        var paymentRecord = await Connection.QueryAsync<PaymentRecordMapping>(
            new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken));

        return paymentRecord.FirstOrDefault();
    }

    public async Task CreateAsync(PaymentRecord paymentRecord, CancellationToken cancellationToken = default)
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
            RETURNING payment_id;
        ";

        const string insertStatusHistorySql = @"
            INSERT INTO payment_status_history (
                payment_id, status, timestamp, reference
            ) VALUES (
                @PaymentId, @Status, @Timestamp, @Reference
            )
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
                    CcLastFourDigits = paymentRecord.GetCreditCardDetails()?.LastFourDigits,
                    CcBrand = paymentRecord.GetCreditCardDetails()?.CardBrand,
                    PaypalEmail = paymentRecord.GetPayPalDetails()?.PayPalEmail,
                    PaypalPayerId = paymentRecord.GetPayPalDetails()?.PayPalPayerId
                },
                cancellationToken: cancellationToken
            )
        );

        foreach (var statusChange in paymentRecord.StatusHistory)
        {
            var statusParameters = new
            {
                PaymentId = paymentId,
                Status = statusChange.Status.ToString(),
                statusChange.Timestamp,
                statusChange.Reference
            };

            await Connection.ExecuteAsync(insertStatusHistorySql, statusParameters, Transaction);
        }
    }
}