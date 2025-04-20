using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Domain.Common;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.UnitTests.Builders;

public class DiscountHistoryBuilder
{
    private Guid _orderId = Guid.NewGuid();
    private string _customerId = "customer123";
    private Guid _discountId = Guid.NewGuid();
    private DiscountType _discountType = DiscountType.Fixed;
    private decimal _discountAmount = 10m;
    private decimal? _percentage = null;
    private decimal? _fixedAmount = 10m;
    private string? _couponCode = null;
    private readonly DateTime _appliedAt = DateTime.UtcNow;
    private Guid? _id = Guid.NewGuid();

    public DiscountHistoryBuilder WithOrderId(Guid orderId)
    {
        _orderId = orderId;
        return this;
    }

    public DiscountHistoryBuilder WithEmptyOrderId()
    {
        _orderId = Guid.Empty;
        return this;
    }

    public DiscountHistoryBuilder WithCustomerId(string customerId)
    {
        _customerId = customerId;
        return this;
    }

    public DiscountHistoryBuilder WithEmptyCustomerId()
    {
        _customerId = string.Empty;
        return this;
    }

    public DiscountHistoryBuilder WithDiscountId(Guid discountId)
    {
        _discountId = discountId;
        return this;
    }

    public DiscountHistoryBuilder WithEmptyDiscountId()
    {
        _discountId = Guid.Empty;
        return this;
    }

    public DiscountHistoryBuilder WithDiscountType(DiscountType discountType)
    {
        _discountType = discountType;
        return this;
    }

    public DiscountHistoryBuilder WithDiscountAmount(decimal discountAmount)
    {
        _discountAmount = discountAmount;
        return this;
    }

    public DiscountHistoryBuilder WithPercentage(decimal? percentage)
    {
        _percentage = percentage;
        return this;
    }

    public DiscountHistoryBuilder WithFixedAmount(decimal? fixedAmount)
    {
        _fixedAmount = fixedAmount;
        return this;
    }

    public DiscountHistoryBuilder WithCouponCode(string? couponCode)
    {
        _couponCode = couponCode;
        return this;
    }

    public DiscountHistoryBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public DiscountHistoryBuilder AsFixedDiscount()
    {
        _discountType = DiscountType.Fixed;
        _fixedAmount = 10m;
        _percentage = null;
        return this;
    }

    public DiscountHistoryBuilder AsPercentageDiscount()
    {
        _discountType = DiscountType.Percentage;
        _fixedAmount = null;
        _percentage = 15m;
        return this;
    }

    public DiscountHistoryBuilder AsCouponDiscount()
    {
        _discountType = DiscountType.Coupon;
        _fixedAmount = 10m;
        _percentage = null;
        _couponCode = "DISCOUNT10";
        return this;
    }

    public Result<DiscountHistory> Build()
    {
        return DiscountHistory.Create(
            _orderId,
            _customerId,
            _discountId,
            _discountType,
            _discountAmount,
            _percentage,
            _fixedAmount,
            _couponCode);
    }

    public Result<DiscountHistory> BuildFromFactory()
    {
        return DiscountHistory.From(
            _id ?? Guid.NewGuid(),
            _orderId,
            _customerId,
            _discountId,
            _discountType,
            _discountAmount,
            _percentage,
            _fixedAmount,
            _couponCode ?? string.Empty,
            _appliedAt);
    }
}