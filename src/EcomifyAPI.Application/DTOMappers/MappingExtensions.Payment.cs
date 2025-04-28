using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsPayment
{
    public static PaymentMethodEnumDTO ToDTO(this PaymentMethodEnum paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethodEnum.CreditCard => PaymentMethodEnumDTO.CreditCard,
            PaymentMethodEnum.PayPal => PaymentMethodEnumDTO.PayPal,
            _ => throw new ArgumentException("Invalid payment method")
        };
    }

    public static PaymentMethodEnum ToDomain(this PaymentMethodEnumDTO paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethodEnumDTO.CreditCard => PaymentMethodEnum.CreditCard,
            PaymentMethodEnumDTO.PayPal => PaymentMethodEnum.PayPal,
            _ => throw new ArgumentException("Invalid payment method")
        };
    }

    public static PaymentStatusDTO ToDTO(this PaymentStatusEnum paymentStatus)
    {
        return paymentStatus switch
        {
            PaymentStatusEnum.Processing => PaymentStatusDTO.Processing,
            PaymentStatusEnum.Succeeded => PaymentStatusDTO.Succeeded,
            PaymentStatusEnum.Failed => PaymentStatusDTO.Failed,
            PaymentStatusEnum.RefundRequested => PaymentStatusDTO.RefundRequested,
            PaymentStatusEnum.Refunded => PaymentStatusDTO.Refunded,
            PaymentStatusEnum.Cancelled => PaymentStatusDTO.Cancelled,
            _ => throw new ArgumentException("Invalid payment status")
        };
    }

    public static PaymentStatusEnum ToDomain(this PaymentStatusDTO paymentStatus)
    {
        return paymentStatus switch
        {
            PaymentStatusDTO.Processing => PaymentStatusEnum.Processing,
            PaymentStatusDTO.Succeeded => PaymentStatusEnum.Succeeded,
            PaymentStatusDTO.Failed => PaymentStatusEnum.Failed,
            PaymentStatusDTO.RefundRequested => PaymentStatusEnum.RefundRequested,
            PaymentStatusDTO.Refunded => PaymentStatusEnum.Refunded,
            PaymentStatusDTO.Cancelled => PaymentStatusEnum.Cancelled,
            _ => throw new ArgumentException("Invalid payment status")
        };
    }

    public static PaginatedResponseDTO<PaymentResponseDTO> ToPaginatedDTO(this FilteredResponseMapping<PaymentRecordMapping> payments,
    int pageNumber, int pageSize)
    {
        return new PaginatedResponseDTO<PaymentResponseDTO>(
            [.. payments.Items.Select(p => p.ToDTO())],
            pageSize,
            pageNumber,
            payments.TotalCount
        );
    }

    public static PaymentStatusHistoryResponseDTO ToDTO(this PaymentRecordHistoryMapping paymentStatusHistory)
    {
        return new PaymentStatusHistoryResponseDTO(
            paymentStatusHistory.Id,
            paymentStatusHistory.Status,
            paymentStatusHistory.Timestamp,
            paymentStatusHistory.Reference);
    }

    public static PaymentStatusHistoryResponseDTO ToDTO(this PaymentStatusChange paymentStatusHistory)
    {
        return new PaymentStatusHistoryResponseDTO(
            paymentStatusHistory.Id,
            paymentStatusHistory.Status.ToDTO(),
            paymentStatusHistory.Timestamp,
            paymentStatusHistory.Reference);
    }

    public static PaymentResponseDTO ToDTO(this PaymentRecordMapping payment)
    {
        return new PaymentResponseDTO(
            payment.TransactionId,
            payment.Amount,
            payment.PaymentMethod,
            payment.ProcessedAt,
            payment.Status,
            payment.GatewayResponse,
            payment.CcLastFourDigits,
            payment.CcBrand,
            payment.PaypalEmail,
            [.. payment.StatusHistory.Select(p => p.ToDTO())]
        );
    }

    public static IReadOnlyList<PaymentResponseDTO> ToDTO(this IEnumerable<PaymentRecordMapping> payments)
    {
        return [.. payments.Select(p => p.ToDTO())];
    }

    public static PaymentRecord ToDomain(this PaymentRecordMapping payment)
    {
        PaymentRecord result;

        if (!string.IsNullOrEmpty(payment.CcLastFourDigits))
        {
            result = PaymentRecord.From(
                payment.PaymentId,
                payment.OrderId,
                new Money(payment.CurrencyCode, payment.Amount),
                payment.PaymentMethod.ToDomain(),
                payment.TransactionId,
                payment.GatewayResponse,
                new CreditCardDetails(payment.CcLastFourDigits, payment.CcBrand)
            );
        }
        else
        {
            result = PaymentRecord.From(
                payment.PaymentId,
                payment.OrderId,
                new Money(payment.CurrencyCode, payment.Amount),
                payment.PaymentMethod.ToDomain(),
                payment.TransactionId,
                payment.GatewayResponse,
                new PayPalDetails(payment.PaypalEmail, payment.PaypalPayerId)
            );
        }

        return result;
    }
}