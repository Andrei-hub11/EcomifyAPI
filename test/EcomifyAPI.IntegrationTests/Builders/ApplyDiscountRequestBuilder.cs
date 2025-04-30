using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class ApplyDiscountRequestBuilder
{
    private Guid _discountId = Guid.Empty;
    private DiscountTypeEnum _discountType = DiscountTypeEnum.Fixed;
    private decimal? _percentage = null;
    private decimal? _fixedAmount = null;
    private string _couponCode = string.Empty;

    public ApplyDiscountRequestBuilder WithDiscountId(Guid discountId)
    {
        _discountId = discountId;
        return this;
    }

    public ApplyDiscountRequestBuilder WithDiscountType(DiscountTypeEnum discountType)
    {
        _discountType = discountType;
        return this;
    }

    public ApplyDiscountRequestBuilder WithPercentage(decimal percentage)
    {
        _percentage = percentage;
        return this;
    }

    public ApplyDiscountRequestBuilder WithFixedAmount(decimal fixedAmount)
    {
        _fixedAmount = fixedAmount;
        return this;
    }

    public ApplyDiscountRequestBuilder WithCouponCode(string couponCode)
    {
        _couponCode = couponCode;
        return this;
    }

    public ApplyDiscountRequestDTO Build()
    {
        return new ApplyDiscountRequestDTO(
            _discountId,
            _discountType,
            _percentage,
            _fixedAmount,
            _couponCode);
    }
}