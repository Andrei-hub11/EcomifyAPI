using EcomifyAPI.Application.Contracts.Contexts;
using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Discounts;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Validation;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.Application.Discounts;

internal class CouponDiscountStrategy : IDiscountStrategyResolver
{
    public DiscountTypeEnum DiscountType => DiscountTypeEnum.Coupon;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IDiscountRepository _discountRepository;

    public CouponDiscountStrategy(IUserContext userContext, IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _discountRepository = _unitOfWork.GetRepository<IDiscountRepository>();
    }

    public async Task<Result<decimal>> ApplyDiscountAsync(decimal orderAmount, ApplyDiscountRequestDTO request,
    CancellationToken cancellationToken = default)
    {
        var validationErrors = DiscountDTOValidation.Validate(
            request.CouponCode,
            request.Percentage,
            request.FixedAmount,
            (int)request.DiscountType
        );

        if (validationErrors.Any())
        {
            return Result.Fail(validationErrors);
        }

        var existingDiscount = await _discountRepository.GetDiscountByCodeAsync(request.CouponCode, cancellationToken);

        if (existingDiscount is null)
        {
            return Result.Fail(DiscountErrorFactory.DiscountNotFoundByCode(request.CouponCode));
        }

        var coupon = Discount.From(
            existingDiscount.Id,
            existingDiscount.Code,
            (DiscountType)existingDiscount.DiscountType,
            existingDiscount.FixedAmount,
            existingDiscount.Percentage,
            existingDiscount.MaxUses,
            existingDiscount.Uses,
            existingDiscount.MinOrderAmount,
            existingDiscount.MaxUsesPerUser,
            existingDiscount.ValidFrom,
            existingDiscount.ValidTo,
            existingDiscount.IsActive,
            existingDiscount.AutoApply,
            existingDiscount.CreatedAt
        );

        if (coupon.IsFailure)
        {
            return Result.Fail(coupon.Errors);
        }

        var userUsages = await _discountRepository.GetUserUsagesAsync(request.CouponCode, cancellationToken);

        if (!coupon.Value.IsValidForUse(orderAmount, userUsages))
        {
            return Result.Fail(DiscountErrorFactory.DiscountNotValidForUse(existingDiscount.Id));
        }

        var discountAmount = coupon.Value.CalculateDiscount(orderAmount);

        return discountAmount;
    }

    public async Task<Result<decimal>> CalculateTotalDiscountAsync(
        decimal cartAmount,
        HashSet<Guid> discountIds,
        CancellationToken cancellationToken = default)
    {
        if (discountIds == null || discountIds.Count == 0)
        {
            return Result.Ok(0m);
        }

        var recentDiscounts = await _discountRepository.GetRecentDiscountsByCustomerIdAsync(
            _userContext.UserId,
            DateTime.UtcNow.AddDays(-7));

        var count = recentDiscounts.Count();

        if (count > 8)
        {
            return Result.Fail("Customer has received too many discounts recently");
        }

        var totalDiscount = 0m;

        foreach (var discountId in discountIds.Distinct())
        {
            var existingDiscount = await _discountRepository.GetDiscountByIdAsync(discountId, cancellationToken);

            if (existingDiscount is null)
            {
                return Result.Fail(DiscountErrorFactory.DiscountNotFoundById(discountId));
            }

            var couponResult = Discount.From(
                existingDiscount.Id,
                existingDiscount.Code,
                (DiscountType)existingDiscount.DiscountType,
                existingDiscount.FixedAmount,
                existingDiscount.Percentage,
                existingDiscount.MaxUses,
                existingDiscount.Uses,
                existingDiscount.MinOrderAmount,
                existingDiscount.MaxUsesPerUser,
                existingDiscount.ValidFrom,
                existingDiscount.ValidTo,
                existingDiscount.IsActive,
                existingDiscount.AutoApply,
                existingDiscount.CreatedAt
            );

            if (couponResult.IsFailure)
            {
                return Result.Fail(couponResult.Errors);
            }

            var discount = couponResult.Value;

            var userUsages = await _discountRepository.GetUserUsagesAsync(_userContext.UserId, cancellationToken);

            if (!discount.IsValidForUse(cartAmount, userUsages))
            {
                return Result.Fail(DiscountErrorFactory.DiscountNotValidForUse(existingDiscount.Id));
            }

            var discountValue = discount.CalculateDiscount(cartAmount - totalDiscount);

            if (discountValue <= 0)
            {
                continue;
            }

            totalDiscount += discountValue;

            if (totalDiscount >= cartAmount)
            {
                totalDiscount = cartAmount;
                break;
            }
        }

        return Result.Ok(totalDiscount);
    }
}