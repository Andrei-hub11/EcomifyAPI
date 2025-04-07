using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly IPaymentMethodFactory _paymentMethodFactory;
    private readonly IOrderService _orderService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IPaymentMethodFactory paymentMethodFactory, IOrderService orderService, IUnitOfWork unitOfWork)
    {
        _paymentMethodFactory = paymentMethodFactory;
        _orderService = orderService;
        _unitOfWork = unitOfWork;
        _paymentRepository = _unitOfWork.GetRepository<IPaymentRepository>();
        _orderRepository = _unitOfWork.GetRepository<IOrderRepository>();
    }

    public async Task<Result<PaymentResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);

        if (payment is null)
        {
            return Result.Fail(PaymentErrorFactory.PaymentNotFound(id));
        }

        return Result.Ok(payment.ToDTO());
    }

    public async Task<Result<IReadOnlyList<PaymentResponseDTO>>> GetAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetAsync(cancellationToken);

        return Result.Ok(payments.ToDTO());
    }

    public async Task<Result<PaymentResponseDTO>> ProcessPaymentAsync(PaymentRequestDTO request, CancellationToken cancellationToken = default)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (existingOrder is null)
        {
            return Result.Fail(OrderErrorFactory.OrderNotFound(request.OrderId));
        }

        var order = Order.From(
            existingOrder.Id,
            existingOrder.UserId,
            existingOrder.OrderDate,
            existingOrder.Status.ToOrderStatusDomain(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            new Address(
                existingOrder.ShippingStreet,
                existingOrder.ShippingNumber,
                existingOrder.ShippingCity,
                existingOrder.ShippingState,
                existingOrder.ShippingZipCode,
                existingOrder.ShippingCountry,
                existingOrder.ShippingComplement
            ),
            new Address(
                existingOrder.BillingStreet,
                existingOrder.BillingNumber,
                existingOrder.BillingCity,
                existingOrder.BillingState,
                existingOrder.BillingZipCode,
                existingOrder.BillingCountry,
                existingOrder.BillingComplement
            ),
            [.. existingOrder.Items.Select(item => new OrderItem(
                    item.ItemId,
                    item.ProductId,
                    item.Quantity,
                    new Money(item.CurrencyCode, item.UnitPrice)))]
        );

        if (order.IsFailure)
        {
            return Result.Fail(order.Errors);
        }

        if (order.Value.Status == OrderStatusEnum.Completed)
        {
            return Result.Fail(OrderErrorFactory.OrderAlreadyCompleted(request.OrderId));
        }

        order.Value.UpdateStatus(OrderStatusEnum.Processing);

        var isOrderUpdated = await _orderService.UpdateOrderAsync(
            order.Value.Id,
            order.Value.ToUpdateOrderRequestDTO(),
            cancellationToken
        );

        if (isOrderUpdated.IsFailure)
        {
            return Result.Fail(isOrderUpdated.Errors);
        }

        var paymentMethod = _paymentMethodFactory.GetPaymentMethod(GetPaymentMethod(request.PaymentMethod));

        if (paymentMethod is null)
        {
            return Result.Fail(PaymentErrorFactory.InvalidPaymentMethod(request.PaymentMethod.ToString()));
        }

        PaymentDetails paymentDetails = request.CreditCardDetails as PaymentDetails
      ?? request.PayPalDetails as PaymentDetails
      ?? throw new InvalidOperationException("Invalid payment method");

        var paymentResult = await paymentMethod.ProcessPaymentAsync(paymentDetails, cancellationToken);

        if (paymentResult.IsFailure)
        {
            order.Value.UpdateStatus(OrderStatusEnum.Failed);

            _ = await _orderService.UpdateOrderAsync(
                order.Value.Id,
                order.Value.ToUpdateOrderRequestDTO(),
                    cancellationToken
                );

            return Result.Fail(PaymentErrorFactory.PaymentFailed());
        }

        Result<PaymentRecord> paymentRecord = default!;

        if (request.PaymentMethod == PaymentMethodEnumDTO.CreditCard && request.CreditCardDetails is CreditCardDetailsDTO creditCardDetails)
        {
            paymentRecord = PaymentRecord.CreateCreditCardPayment(
            request.OrderId,
            new Money("BRL", request.Amount),
            paymentResult.Value.TransactionId,
            creditCardDetails.CardNumber.Substring(creditCardDetails.CardNumber.Length - 4),
            paymentResult.Value.Reference,
            GetCardBrand(creditCardDetails.CardNumber)
            );
        }

        if (request.PaymentMethod == PaymentMethodEnumDTO.PayPal && request.PayPalDetails is PayPalDetailsDTO payPalDetails)
        {
            paymentRecord = PaymentRecord.CreatePayPalPayment(
                request.OrderId,
                new Money("BRL", request.Amount),
                paymentResult.Value.TransactionId,
                payPalDetails.PayerId.ToString(),
                payPalDetails.PayerEmail,
                paymentResult.Value.Reference
            );
        }

        if (paymentRecord.IsFailure)
        {
            return Result.Fail(paymentRecord.Errors);
        }

        paymentRecord.Value.MarkAsSucceeded(paymentResult.Value.Reference);

        await _paymentRepository.CreateAsync(paymentRecord.Value, cancellationToken);

        order.Value.UpdateStatus(OrderStatusEnum.Confirmed);

        var isOrderConfirmed = await _orderService.UpdateOrderAsync(
            order.Value.Id,
            order.Value.ToUpdateOrderRequestDTO(),
            cancellationToken
        );

        if (isOrderConfirmed.IsFailure)
        {
            return Result.Fail(isOrderConfirmed.Errors);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(new PaymentResponseDTO(
            paymentRecord.Value.TransactionId,
            paymentRecord.Value.Amount.Amount,
            paymentRecord.Value.PaymentMethod.ToDTO(),
            paymentRecord.Value.ProcessedAt,
            paymentRecord.Value.Status.ToDTO(),
            paymentRecord.Value.GatewayResponse,
            paymentRecord.Value.GetCreditCardDetails()?.LastFourDigits,
            paymentRecord.Value.GetCreditCardDetails()?.CardBrand,
            paymentRecord.Value.GetPayPalDetails()?.PayPalEmail.Value,
            paymentRecord.Value.StatusHistory.Select(statusHistory => statusHistory.ToDTO()).ToList()
        ));
    }

    public Task<Result<bool>> RefundPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> CancelPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }


    private string GetPaymentMethod(PaymentMethodEnumDTO paymentMethod)
    {
        return paymentMethod switch
        {
            PaymentMethodEnumDTO.CreditCard => "CreditCard",
            PaymentMethodEnumDTO.PayPal => "PayPal",
            _ => throw new ArgumentException("Invalid payment method")
        };
    }

    public string GetCardBrand(string cardNumber)
    {
        return cardNumber switch
        {
            _ when cardNumber.StartsWith("4") => "Visa",
            _ when cardNumber.StartsWith("5") => "MasterCard",
            _ when cardNumber.StartsWith("3") => "American Express",
            _ => "Unknown"
        };
    }
}