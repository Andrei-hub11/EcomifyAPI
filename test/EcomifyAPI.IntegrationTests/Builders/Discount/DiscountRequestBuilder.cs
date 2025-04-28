using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class CreateDiscountRequestBuilder
{
    private string? _couponCode = null;
    private decimal? _fixedAmount = null;
    private decimal? _percentage = null;
    private DateTime _validFrom = DateTime.UtcNow.AddDays(1);
    private DateTime _validTo = DateTime.UtcNow.AddDays(30);
    private int _maxUses = 100;
    private int _uses = 0;
    private decimal _minOrderAmount = 50.00m;
    private int _maxUsesPerUser = 1;
    private bool _autoApply = false;
    private DiscountTypeEnum _discountType = DiscountTypeEnum.Fixed;
    private HashSet<Guid> _categories = new();

    public CreateDiscountRequestBuilder WithCouponCode(string? couponCode)
    {
        _couponCode = couponCode;
        return this;
    }

    public CreateDiscountRequestBuilder WithFixedAmount(decimal? fixedAmount)
    {
        _fixedAmount = fixedAmount;
        return this;
    }

    public CreateDiscountRequestBuilder WithPercentage(decimal? percentage)
    {
        _percentage = percentage;
        return this;
    }

    public CreateDiscountRequestBuilder WithValidFrom(DateTime validFrom)
    {
        _validFrom = validFrom;
        return this;
    }

    public CreateDiscountRequestBuilder WithValidTo(DateTime validTo)
    {
        _validTo = validTo;
        return this;
    }

    public CreateDiscountRequestBuilder WithMaxUses(int maxUses)
    {
        _maxUses = maxUses;
        return this;
    }

    public CreateDiscountRequestBuilder WithUses(int uses)
    {
        _uses = uses;
        return this;
    }

    public CreateDiscountRequestBuilder WithMinOrderAmount(decimal minOrderAmount)
    {
        _minOrderAmount = minOrderAmount;
        return this;
    }

    public CreateDiscountRequestBuilder WithMaxUsesPerUser(int maxUsesPerUser)
    {
        _maxUsesPerUser = maxUsesPerUser;
        return this;
    }

    public CreateDiscountRequestBuilder WithAutoApply(bool autoApply)
    {
        _autoApply = autoApply;
        return this;
    }

    public CreateDiscountRequestBuilder WithDiscountType(DiscountTypeEnum discountType)
    {
        _discountType = discountType;
        return this;
    }

    public CreateDiscountRequestBuilder WithCategories(params Guid[] categoryIds)
    {
        _categories = categoryIds.ToHashSet();
        return this;
    }

    public CreateDiscountRequestDTO Build()
    {
        return new CreateDiscountRequestDTO(
            _couponCode,
            _fixedAmount,
            _percentage,
            _validFrom,
            _validTo,
            _maxUses,
            _uses,
            _minOrderAmount,
            _maxUsesPerUser,
            _autoApply,
            _discountType,
            _categories
        );
    }
}