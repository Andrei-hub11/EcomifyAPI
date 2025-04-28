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

internal class FixedAmountDiscountStrategy : IDiscountStrategyResolver
{
    public DiscountTypeEnum DiscountType => DiscountTypeEnum.Fixed;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDiscountRepository _discountRepository;

    public FixedAmountDiscountStrategy(IUserContext userContext, IUnitOfWork unitOfWork)
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

        var existingDiscount = await _discountRepository.GetDiscountByIdAsync(request.DiscountId, cancellationToken);

        if (existingDiscount is null)
        {
            return Result.Fail(DiscountErrorFactory.DiscountNotFoundById(request.DiscountId));
        }

        var fixedAmount = existingDiscount.FixedAmount;

        // Ensure the discount doesn't exceed the order amount
        if (fixedAmount > orderAmount)
        {
            fixedAmount = orderAmount;
        }

        return fixedAmount;
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
            _userContext.UserId, DateTime.UtcNow.AddDays(-7));

        var count = recentDiscounts.Count();

        if (count > 8)
        {
            return Result.Fail("Customer has received too many discounts recently");
        }

        var totalDiscount = 0m;

        foreach (var discountId in discountIds)
        {
            var existingDiscount = await _discountRepository.GetDiscountByIdAsync(discountId, cancellationToken);

            if (existingDiscount is null)
            {
                return Result.Fail(DiscountErrorFactory.DiscountNotFoundById(discountId));
            }

            var discount = Discount.From(
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

            if (discount.IsFailure)
            {
                return Result.Fail(discount.Errors);
            }

            if (existingDiscount.MinOrderAmount > cartAmount)
            {
                return Result.Fail(DiscountErrorFactory.MinimumOrderAmountNotReached(existingDiscount.MinOrderAmount));
            }

            var userUsages = await _discountRepository.GetUserUsagesAsync(_userContext.UserId, cancellationToken);

            if (!discount.Value.IsValidForUse(cartAmount, userUsages))
            {
                return Result.Fail(DiscountErrorFactory.DiscountNotValidForUse(existingDiscount.Id));
            }

            var discountValue = existingDiscount.FixedAmount;

            if (discountValue > (cartAmount - totalDiscount))
            {
                discountValue = cartAmount - totalDiscount;
            }

            totalDiscount += discountValue ?? 0;

            if (totalDiscount >= cartAmount)
            {
                totalDiscount = cartAmount;
                break; // não dá pra descontar mais do que o valor total
            }
        }

        return Result.Ok(totalDiscount);
    }
}