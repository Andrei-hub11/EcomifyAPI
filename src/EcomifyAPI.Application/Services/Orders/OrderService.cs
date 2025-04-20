using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Discounts;
using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Services.Orders;

public sealed class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductService _productService;
    private readonly IAccountService _accountService;
    private readonly IDiscountService _discountService;
    private readonly ICartService _cartService;
    private readonly IDiscountServiceFactory _discountServiceFactory;
    private readonly ILoggerHelper<OrderService> _logger;

    public OrderService(
        IUnitOfWork unitOfWork,
        IProductService productService,
        IAccountService accountService,
        IDiscountService discountService,
        ICartService cartService,
        IDiscountServiceFactory discountServiceFactory,
        ILoggerHelper<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = unitOfWork.GetRepository<IOrderRepository>();
        _productService = productService;
        _accountService = accountService;
        _discountService = discountService;
        _cartService = cartService;
        _discountServiceFactory = discountServiceFactory;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<OrderResponseDTO>>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var orders = await _orderRepository.GetAsync(cancellationToken);

            return Result.Ok(orders.ToResponseDTO());
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<OrderResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

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
            var user = await _accountService.GetByIdAsync(request.UserId, cancellationToken);

            if (user.IsFailure)
            {
                return Result.Fail(user.Errors);
            }

            var existingCart = await _cartService.GetCartAsync(request.UserId, cancellationToken);

            if (existingCart.IsFailure)
            {
                return Result.Fail(existingCart.Errors);
            }

            var order = Order.Create(
                request.UserId,
                DateTime.UtcNow,
                OrderStatusEnum.Confirmed,
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

            if (existingCart.Value.Items.Count == 0)
            {
                return Result.Fail(OrderErrorFactory.CartEmpty());
            }

            foreach (var item in existingCart.Value.Items)
            {
                var productDTO = await _productService.GetByIdAsync(item.ProductId, cancellationToken);

                if (productDTO.IsFailure)
                {
                    return Result.Fail(productDTO.Errors);
                }

                var product = productDTO.Value.ToDomain();

                if (product.Stock < item.Quantity)
                {
                    return Result.Fail($"Insufficient stock for product with id = '{item.ProductId}'");
                }

                order.Value.AddItem(product, item.Quantity, new Money(item.UnitPrice.Code, item.UnitPrice.Amount));

                if (!product.DecrementStock(item.Quantity))
                {
                    return Result.Fail(ProductErrorFactory.ProductOutOfStock(item.ProductId));
                }
            }

            var currencyCode = order.Value.TotalAmount.Code;

            var discountsToApply = await _discountService.GetDiscountToApply(request.UserId, cancellationToken);

            if (discountsToApply.IsFailure)
            {
                return Result.Fail(discountsToApply.Errors);
            }

            var discountIds = discountsToApply.Value.Select(d => d.Id).ToList();

            var discountHistories = new List<DiscountHistory>();

            foreach (var discount in discountsToApply.Value)
            {
                // Calculate discount amount using the discount service factory
                var strategy = _discountServiceFactory.GetDiscountService(discount.DiscountType);
                var discountResult = await strategy.CalculateTotalDiscountAsync(
                    order.Value.TotalAmount.Amount,
                    [discount.Id],
                    cancellationToken);

                if (discountResult.IsFailure)
                {
                    return Result.Fail(discountResult.Errors);
                }

                var discountAmount = discountResult.Value;

                if (discountAmount > 0)
                {
                    order.Value.ApplyDiscount(discountAmount);

                    // Create discount history record
                    var discountHistory = DiscountHistory.Create(
                        order.Value.Id,
                            request.UserId,
                            discount.Id,
                            (DiscountType)discount.DiscountType,
                            discountAmount,
                            discount.Percentage,
                            discount.FixedAmount,
                            discount.Code);

                    if (discountHistory.IsFailure)
                    {
                        return Result.Fail(discountHistory.Errors);
                    }

                    discountHistories.Add(discountHistory.Value);

                    _logger.LogInformation($"Applied discounts to order. Total discount amount: {discountAmount}");
                }
            }

            foreach (var discountHistory in discountHistories)
            {
                await _discountService.CreateDiscountHistoryAsync(discountHistory, cancellationToken);
            }

            var orderId = await _orderRepository.CreateAsync(order.Value, currencyCode, cancellationToken);

            foreach (var item in order.Value.OrderItems)
            {
                await _orderRepository.CreateOrderItemAsync(item, orderId, cancellationToken);
            }

            await _accountService.GetOrCreateUserAddressAsync(request.UserId,
                new CreateAddressRequestDTO(request.ShippingAddress), cancellationToken);

            await _cartService.ClearCartAsync(request.UserId, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<bool>> UpdateStatusAsync(Guid orderId, OrderStatusEnum status, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingOrder = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

            if (existingOrder is null)
            {
                return Result.Fail(OrderErrorFactory.OrderNotFound(orderId));
            }

            var orderDomain = existingOrder.ToDomain();

            orderDomain.UpdateStatus(status);

            await _orderRepository.UpdateAsync(orderDomain, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<bool>> DeleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id, cancellationToken);

            if (order is null)
            {
                return Result.Fail(OrderErrorFactory.OrderNotFound(id));
            }

            await _orderRepository.DeleteAsync(id, cancellationToken);

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