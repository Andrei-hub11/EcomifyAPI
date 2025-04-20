using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Discounts;
using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.DTOMappers;
using EcomifyAPI.Common.Extensions.Enums;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Services.Carts;

public class CartService : ICartService
{
    private readonly IAccountService _accountService;
    private readonly ILoggerHelper<CartService> _logger;
    private readonly IDiscountServiceFactory _discountServiceFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDiscountRepository _discountRepository;

    public CartService(IAccountService accountService, ILoggerHelper<CartService> logger,
    IDiscountServiceFactory discountServiceFactory, IUnitOfWork unitOfWork)
    {
        _accountService = accountService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _discountServiceFactory = discountServiceFactory;
        _cartRepository = _unitOfWork.GetRepository<ICartRepository>();
        _productRepository = _unitOfWork.GetRepository<IProductRepository>();
        _discountRepository = _unitOfWork.GetRepository<IDiscountRepository>();
    }

    public async Task<Result<CartResponseDTO>> GetCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await UserExistsAsync(userId, cancellationToken))
            {
                return Result.Fail(UserErrorFactory.UserNotFoundById(userId));
            }

            var cart = await _cartRepository.GetCartAsync(userId, cancellationToken);

            var items = cart is not null && cart.Items.Count != 0 ? cart.Items.Select(i => i.ProductId) : [];

            var autoApplicableDiscounts = await _cartRepository.GetAutoApplicableDiscountsAsync(
                items, cancellationToken);

            var newDiscounts = autoApplicableDiscounts.Where(d => !cart?.Discounts.Any(cd => cd.Id == d.Id) ?? true)
            .Select(d => d.ToDomain()).ToList();

            var cartResult = cart is null ? Cart.Create(userId) : Cart.From(
                cart.Id, userId, cart.CreatedAt, cart.UpdatedAt,
                cart.Items.ToDomain(),
                [.. cart.Discounts.ToDomain(), .. newDiscounts]);

            if (cartResult.IsFailure)
            {
                return Result.Fail(cartResult.Errors);
            }

            if (cart is null)
            {
                await _cartRepository.CreateCartAsync(cartResult.Value, cancellationToken);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return cartResult.Value.ToDTO();
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<CartResponseDTO>> AddItemAsync(string userId, Guid productId, int quantity,
    CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await UserExistsAsync(userId, cancellationToken))
            {
                return Result.Fail(UserErrorFactory.UserNotFoundById(userId));
            }

            var cart = await _cartRepository.GetCartAsync(userId);

            if (cart is null)
            {
                return CartErrorFactory.CartNotFound(userId);
            }

            var cartResult = Cart.From(
                cart.Id, userId, cart.CreatedAt, cart.UpdatedAt, cart.Items.ToDomain(),
                cart.Discounts.ToDomain());

            if (cartResult.IsFailure)
            {
                return CartErrorFactory.CartNotFound(userId);
            }

            var existingProduct = await _productRepository.GetByIdAsync(productId);

            if (existingProduct is null)
            {
                return ProductErrorFactory.ProductNotFoundById(productId);
            }

            var product = Product.From(
                existingProduct.Id,
                existingProduct.Name,
                existingProduct.Description,
                existingProduct.Price,
                existingProduct.CurrencyCode,
                existingProduct.Stock,
                existingProduct.ImageUrl,
                existingProduct.Status.ToProductStatusDomain()
            );

            if (product.IsFailure)
            {
                return Result.Fail(product.Errors);
            }

            cartResult.Value.AddItem(product.Value, quantity, new Money("BRL", product.Value.Price.Amount));
            await _cartRepository.AddItemAsync(cartResult.Value.Id, cartResult.Value.Items[^1], cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return cartResult.Value.ToDTO();
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<CartResponseDTO>> ApplyDiscountAsync(string userId, ApplyDiscountRequestDTO request,
    CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await UserExistsAsync(userId, cancellationToken))
            {
                return Result.Fail(UserErrorFactory.UserNotFoundById(userId));
            }

            var cart = await _cartRepository.GetCartAsync(userId);

            if (cart is null)
            {
                return CartErrorFactory.CartNotFound(userId);
            }

            var cartResult = Cart.From(
                cart.Id, userId, cart.CreatedAt, cart.UpdatedAt, cart.Items.ToDomain(),
                cart.Discounts.ToDomain());

            if (cartResult.IsFailure)
            {
                return Result.Fail(cartResult.Errors);
            }

            var discount = await _discountRepository.GetDiscountByIdAsync(request.DiscountId, cancellationToken);

            if (discount is null)
            {
                return DiscountErrorFactory.DiscountNotFoundById(request.DiscountId);
            }

            var strategy = _discountServiceFactory.GetDiscountService(discount.DiscountType);

            var discountResult = await strategy.CalculateTotalDiscountAsync(cartResult.Value.TotalAmount.Amount,
                [request.DiscountId], cancellationToken);

            if (discountResult.IsFailure)
            {
                return Result.Fail(discountResult.Errors);
            }

            cartResult.Value.UpdateTotalWithDiscount(discountResult.Value);
            await _discountRepository.ApplyDiscountToCartAsync(cartResult.Value.Id, discount.Id, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Applied '{DiscountType}' discount to user with ID '{UserId}'. Discount amount: {DiscountAmount}",
            ((DiscountType)discount.DiscountType).GetDescription(), userId, discountResult.Value);

            return cartResult.Value.ToDTO();
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<CartResponseDTO>> RemoveItemAsync(string userId, Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await UserExistsAsync(userId, cancellationToken))
            {
                return Result.Fail(UserErrorFactory.UserNotFoundById(userId));
            }

            var cart = await _cartRepository.GetCartAsync(userId);

            if (cart is null)
            {
                return CartErrorFactory.CartNotFound(userId);
            }

            var cartResult = Cart.From(
                cart.Id, userId, cart.CreatedAt, cart.UpdatedAt, cart.Items.ToDomain(),
                cart.Discounts.ToDomain());

            if (cartResult.IsFailure)
            {
                return Result.Fail(cartResult.Errors);
            }

            var itemExists = cartResult.Value.Items.FirstOrDefault(i => i.ProductId == productId);

            if (itemExists is null)
            {
                return CartErrorFactory.CartItemNotFound(productId);
            }

            cartResult.Value.RemoveItem(productId);
            await _cartRepository.RemoveItemAsync(cartResult.Value.Id, productId, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return cartResult.Value.ToDTO();
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<CartResponseDTO>> UpdateItemQuantityAsync(string userId, Guid productId, int quantity,
    CancellationToken cancellationToken = default)
    {
        if (!await UserExistsAsync(userId, cancellationToken))
        {
            return Result.Fail(UserErrorFactory.UserNotFoundById(userId));
        }

        var cart = await _cartRepository.GetCartAsync(userId);

        if (cart is null)
        {
            return CartErrorFactory.CartNotFound(userId);
        }

        var cartResult = Cart.From(
            cart.Id, userId, cart.CreatedAt, cart.UpdatedAt, cart.Items.ToDomain(),
            cart.Discounts.ToDomain());

        if (cartResult.IsFailure)
        {
            return Result.Fail(cartResult.Errors);
        }

        var item = cartResult.Value.Items.FirstOrDefault(i => i.ProductId == productId);

        if (item is null)
        {
            return ProductErrorFactory.ProductNotFoundById(productId);
        }

        item.UpdateQuantity(quantity);

        await _cartRepository.UpdateItemQuantityAsync(cartResult.Value.Id, productId, item, cancellationToken);

        return cartResult.Value.ToDTO();
    }

    public async Task<Result<CartResponseDTO>> ClearCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (!await UserExistsAsync(userId, cancellationToken))
        {
            return Result.Fail(UserErrorFactory.UserNotFoundById(userId));
        }

        var cart = await _cartRepository.GetCartAsync(userId);

        if (cart is null)
        {
            return CartErrorFactory.CartNotFound(userId);
        }

        var cartResult = Cart.From(
            cart.Id, userId, cart.CreatedAt, cart.UpdatedAt, cart.Items.ToDomain(),
            cart.Discounts.ToDomain());

        if (cartResult.IsFailure)
        {
            return Result.Fail(cartResult.Errors);
        }

        cartResult.Value.Clear();
        await _cartRepository.ClearCartAsync(cartResult.Value.Id, cancellationToken);

        return cartResult.Value.ToDTO();
    }

    private async Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _accountService.GetByIdAsync(userId, cancellationToken);

        if (user.IsFailure && user.Errors.Any(e => e.ErrorType == ErrorType.NotFound))
        {
            return false;
        }

        if (user.IsFailure && user.Errors.Any(e => e.ErrorType != ErrorType.NotFound))
        {
            throw new BadRequestException(string.Join(", ", user.Errors.Select(e => e.Description)));
        }

        return true;
    }
}