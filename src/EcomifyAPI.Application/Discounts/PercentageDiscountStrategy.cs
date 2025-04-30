using EcomifyAPI.Application.Contracts.Contexts;
using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Discounts;
using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Common.Utils.Errors;
using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Validation;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.Application.Discounts;

internal class PercentageDiscountStrategy : IDiscountStrategyResolver
{
    public DiscountTypeEnum DiscountType => DiscountTypeEnum.Percentage;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IDiscountRepository _discountRepository;
    private readonly ILoggerHelper<PercentageDiscountStrategy> _logger;
    private readonly IUserContext _userContext;

    private const decimal MAX_ALLOWED_PERCENTAGE = 50.0m;

    public PercentageDiscountStrategy(
        IUnitOfWork unitOfWork,
        ILoggerHelper<PercentageDiscountStrategy> logger,
        IUserContext userContext)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = _unitOfWork.GetRepository<IOrderRepository>();
        _discountRepository = _unitOfWork.GetRepository<IDiscountRepository>();
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<Result<decimal>> ApplyDiscountAsync(decimal orderAmount, ApplyDiscountRequestDTO request,
    CancellationToken cancellationToken = default)
    {
        try
        {
            var discountValidation = DiscountDTOValidation.Validate(
                request.CouponCode,
                request.Percentage,
                request.FixedAmount,
                (int)request.DiscountType
            );

            if (discountValidation.Any())
            {
                return Result.Fail(discountValidation);
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

            bool isAdmin = _userContext.IsAdmin;

            // Limit the maximum discount percentage for non-admin users
            if (!isAdmin && request?.Percentage > MAX_ALLOWED_PERCENTAGE)
            {
                return Result.Fail($"Maximum allowed discount percentage for non-admin users is {MAX_ALLOWED_PERCENTAGE}%");
            }

            var recentDiscounts = await _discountRepository.GetRecentDiscountsByCustomerIdAsync(
                _userContext.UserId,
                DateTime.UtcNow.AddDays(-1));

            var count = recentDiscounts.Count();

            if (count > 8)
            {
                return Result.Fail("Customer has received too many discounts recently");
            }

            var userUsages = await _discountRepository.GetUserUsagesAsync(_userContext.UserId, cancellationToken);

            if (!coupon.Value.IsValidForUse(orderAmount, userUsages))
            {
                return Result.Fail(DiscountErrorFactory.DiscountNotValidForUse(existingDiscount.Id));
            }

            // Calculate the discounted value
            decimal discountedValue = coupon.Value.CalculateDiscount(orderAmount);

            // Ensure minimum value (to avoid 100% discounts)
            if (discountedValue < 0.01m)
                discountedValue = 0.01m;

            /*             // Registrar a aplicação do desconto para auditoria
                        await _discountHistoryRepository.CreateAsync(new DiscountHistory
                        {
                            OrderId = orderId,
                            CustomerId = order.CustomerId,
                            DiscountType = discountType,
                            OriginalAmount = discountAmount,
                            DiscountedAmount = discountedValue,
                            PercentageValue = percentage.Value,
                            CouponCode = couponCode,
                            AppliedAt = appliedAt,
                            AppliedByUserId = currentUser?.Id
                        });

                        */

            return discountedValue;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<decimal>> CalculateTotalDiscountAsync(
     decimal cartAmount,
     HashSet<Guid> discountIds,
     CancellationToken cancellationToken = default)
    {
        try
        {
            if (discountIds == null || discountIds.Count == 0)
            {
                return Result.Ok(0m);
            }

            var recentDiscounts = await _discountRepository.GetRecentDiscountsByCustomerIdAsync(
                _userContext.UserId,
                DateTime.UtcNow.AddDays(-7));

            if (recentDiscounts.Count() > 8)
            {
                return Result.Fail("Customer has received too many discounts recently");
            }

            var totalDiscount = 0m;
            var userUsages = await _discountRepository.GetUserUsagesAsync(_userContext.UserId, cancellationToken);

            foreach (var discountId in discountIds)
            {
                var discountData = await _discountRepository.GetDiscountByIdAsync(discountId, cancellationToken);

                if (discountData is null)
                    return Result.Fail(DiscountErrorFactory.DiscountNotFoundById(discountId));

                var couponResult = Discount.From(
                    discountData.Id,
                    discountData.Code,
                    (DiscountType)discountData.DiscountType,
                    discountData.FixedAmount,
                    discountData.Percentage,
                    discountData.MaxUses,
                    discountData.Uses,
                    discountData.MinOrderAmount,
                    discountData.MaxUsesPerUser,
                    discountData.ValidFrom,
                    discountData.ValidTo,
                    discountData.IsActive,
                    discountData.AutoApply,
                    discountData.CreatedAt
                );

                if (couponResult.IsFailure)
                {
                    return Result.Fail(couponResult.Errors);
                }

                var coupon = couponResult.Value;

                if (discountData.MinOrderAmount > cartAmount)
                {
                    return Result.Fail(DiscountErrorFactory.MinimumOrderAmountNotReached(discountData.MinOrderAmount));
                }

                if (!coupon.IsValidForUse(cartAmount, userUsages))
                {
                    return Result.Fail(DiscountErrorFactory.DiscountNotValidForUse(discountData.Id));
                }

                var percentage = discountData.Percentage ?? 0m;
                var calculatedDiscount = Math.Round(cartAmount * (percentage / 100), 2);

                var remainingAmount = cartAmount - totalDiscount;
                totalDiscount += Math.Min(calculatedDiscount, remainingAmount);

                if (totalDiscount >= cartAmount)
                {
                    totalDiscount = cartAmount;
                    break;
                }
            }

            return Result.Ok(totalDiscount);
        }
        catch (Exception)
        {
            throw;
        }
    }
}