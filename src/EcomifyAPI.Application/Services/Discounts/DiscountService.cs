
using EcomifyAPI.Application.Contracts.Data;
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

namespace EcomifyAPI.Application.Services.Discounts;

public sealed class DiscountService : IDiscountService
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDiscountRepository _discountRepository;

    public DiscountService(IProductService productService, ICartService cartService, IUnitOfWork unitOfWork)
    {
        _productService = productService;
        _cartService = cartService;
        _unitOfWork = unitOfWork;
        _discountRepository = _unitOfWork.GetRepository<IDiscountRepository>();
    }

    public async Task<Result<PaginatedResponseDTO<DiscountResponseDTO>>> GetAllAsync(DiscountFilterRequestDTO filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var discounts = await _discountRepository.GetAllDiscountsAsync(filter, cancellationToken);

            return discounts.ToResponseDTO(filter.PageNumber, filter.PageSize);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<DiscountResponseDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var discount = await _discountRepository.GetDiscountByIdAsync(id, cancellationToken);

            if (discount is null)
            {
                return DiscountErrorFactory.DiscountNotFoundById(id);
            }

            return discount.ToResponseDTO();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<IReadOnlyList<DiscountHistoryResponseDTO>>> GetDiscountHistoryByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var discountHistory = await _discountRepository.GetDiscountHistoryByOrderIdAsync(orderId, cancellationToken);

            if (discountHistory is null)
            {
                return DiscountErrorFactory.DiscountNotFoundById(orderId);
            }

            return Result.Ok(discountHistory.ToResponseDTO());
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<IReadOnlyList<DiscountResponseDTO>>> GetDiscountToApply(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cart = await _cartService.GetCartAsync(userId, cancellationToken);

            if (cart.IsFailure)
            {
                return Result.Fail(cart.Errors);
            }

            var discount = await _discountRepository.GetDiscountToApply(cart.Value.Id, cart.Value.TotalAmount.Amount, cancellationToken);

            if (discount is null)
            {
                return DiscountErrorFactory.DiscountNotFoundById(cart.Value.Id);
            }

            return Result.Ok(discount.ToResponseDTO());
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<DiscountResponseDTO>> CreateAsync(CreateDiscountRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            var discount = Discount.Create(
                request.CouponCode,
                (DiscountType)request.DiscountType,
                request.FixedAmount,
                request.Percentage,
                request.MaxUses,
                request.MinOrderAmount,
                request.MaxUsesPerUser,
                request.ValidFrom,
                request.ValidTo,
                request.AutoApply
            );

            if (discount.IsFailure)
            {
                return Result.Fail(discount.Errors);
            }

            var discountId = await _discountRepository.CreateDiscountAsync(discount.Value, cancellationToken);

            if (request.Categories.Count != 0 && request.AutoApply)
            {
                foreach (var category in request.Categories)
                {
                    await _discountRepository.LinkDiscountToCategoryAsync(discountId, category, cancellationToken);
                }
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            var newDiscount = Discount.From(
                discountId,
                discount.Value.Code,
                discount.Value.DiscountType,
                discount.Value.FixedAmount,
                discount.Value.Percentage,
                discount.Value.MaxUses,
                discount.Value.Uses,
                discount.Value.MinOrderAmount,
                discount.Value.MaxUsesPerUser,
                discount.Value.ValidFrom,
                discount.Value.ValidTo,
                discount.Value.IsActive,
                discount.Value.AutoApply,
                discount.Value.CreatedAt);

            var categories = new HashSet<CategoryResponseDTO>();

            foreach (var categoryId in request.Categories)
            {
                var category = await _productService.GetCategoryByIdAsync(categoryId, cancellationToken);

                if (category.IsFailure)
                {
                    return Result.Fail(category.Errors);
                }

                categories.Add(category.Value);
            }

            return Result.Ok(newDiscount.Value!.ToResponseDTO(categories));
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<bool>> CreateDiscountHistoryAsync(DiscountHistory discountHistory,
    CancellationToken cancellationToken = default)
    {
        try
        {
            await _discountRepository.CreateDiscountHistoryAsync(discountHistory, cancellationToken);

            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var discount = await _discountRepository.GetDiscountByIdAsync(id, cancellationToken);

            if (discount is null)
            {
                return DiscountErrorFactory.DiscountNotFoundById(id);
            }

            await _discountRepository.DeleteDiscountAsync(id, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}