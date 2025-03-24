using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Services.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;
    private readonly ILoggerHelper<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, IProductService productService, ILoggerHelper<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = unitOfWork.GetRepository<IOrderRepository>();
        _productService = productService;
        _logger = logger;
    }

    public async Task<Result<OrderResponseDTO>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(id, cancellationToken);

            if (order is null)
            {
                return Result.Fail(OrderErrorFactory.OrderNotFound(id));
            }

            return order.ToResponseDTO();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<bool>> CreateOrderAsync(CreateOrderRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = Order.Create(Guid.NewGuid(),
            request.UserId, DateTime.UtcNow,
            request.Status,
            DateTime.UtcNow,
            DateTime.UtcNow,
            new Address(request.ShippingAddress.Street,
            request.ShippingAddress.Number,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.ZipCode,
            request.ShippingAddress.Country,
            request.ShippingAddress.Complement),
            new Address(request.BillingAddress.Street,
            request.BillingAddress.Number,
            request.BillingAddress.City,
            request.BillingAddress.State,
            request.BillingAddress.ZipCode,
            request.BillingAddress.Country,
            request.BillingAddress.Complement)
            );


            if (order.IsFailure)
            {
                return Result.Fail(order.Errors);
            }

            foreach (var item in request.Items)
            {
                var productDTO = await _productService.GetProductByIdAsync(item.ProductId, cancellationToken);

                if (productDTO.IsFailure)
                {
                    return Result.Fail(productDTO.Errors);
                }

                var product = productDTO.Value.ToDomain();

                if (product.Stock < item.Quantity)
                {
                    return Result.Fail($"Insufficient stock for product with id = '{item.ProductId}'");
                }

                order.Value.AddItem(product, item.Quantity, new Currency(item.UnitPrice.Code, item.UnitPrice.Amount));

                if (!product.DecrementStock(item.Quantity))
                {
                    return Result.Fail(ProductErrorFactory.ProductOutOfStock(item.ProductId));
                }
            }

            var currencyCode = order.Value.TotalAmount.Code;

            var orderId = await _orderRepository.CreateOrderAsync(order.Value, currencyCode, cancellationToken);

            foreach (var item in order.Value.OrderItems)
            {
                await _orderRepository.CreateOrderItemAsync(item, orderId, cancellationToken);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}