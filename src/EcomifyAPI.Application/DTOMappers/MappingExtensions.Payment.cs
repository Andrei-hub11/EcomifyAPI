using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Response;
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
            PaymentStatusEnum.Unknown => PaymentStatusDTO.Unknown,
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
            PaymentStatusDTO.Unknown => PaymentStatusEnum.Unknown,
            _ => throw new ArgumentException("Invalid payment status")
        };
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
        return [.. payments.Select(payment => payment.ToDTO())];
    }
}