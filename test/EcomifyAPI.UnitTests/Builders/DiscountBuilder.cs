using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.Enums;

namespace EcomifyAPI.UnitTests.Builders;

public class DiscountBuilder
{
    private Guid? _id;
    private string _code = "DEFAULTCODE";
    private DiscountType _discountType = DiscountType.Fixed;
    private decimal? _fixedAmount = 10;
    private decimal? _percentage = 10;
    private int _maxUses = 10;
    private int _uses = 0;
    private decimal _minOrderAmount = 0;
    private int _maxUsesPerUser = 1;
    private DateTime _validFrom = DateTime.UtcNow.AddDays(1);
    private DateTime _validTo = DateTime.UtcNow.AddDays(7);
    private bool _isActive = true;
    private bool _autoApply = false;
    private DateTime _createdAt = DateTime.UtcNow;

    public DiscountBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public DiscountBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public DiscountBuilder WithDiscountType(DiscountType type)
    {
        _discountType = type; return this;
    }

    public DiscountBuilder WithMaxUses(int maxUses)
    {
        _maxUses = maxUses;
        return this;
    }

    public DiscountBuilder WithFixedAmount(decimal? amount)
    {
        _fixedAmount = amount;
        return this;
    }

    public DiscountBuilder WithPercentage(decimal? percentage)
    {
        _percentage = percentage;
        return this;
    }

    public DiscountBuilder WithMaxUsesPerUser(int max)
    {
        _maxUsesPerUser = max;
        return this;
    }

    public DiscountBuilder WithValidTo(DateTime to)
    {
        _validTo = to;
        return this;
    }

    public DiscountBuilder WithIsActive(bool active)
    {
        _isActive = active;
        return this;
    }

    public DiscountBuilder WithUses(int uses)
    {
        _uses = uses;
        return this;
    }
    public DiscountBuilder WithMinOrderAmount(decimal amount)
    {
        _minOrderAmount = amount;
        return this;
    }

    public DiscountBuilder WithAutoApply(bool autoApply)
    {
        _autoApply = autoApply;
        return this;
    }

    public DiscountBuilder WithValidFrom(DateTime from)
    {
        _validFrom = from;
        return this;
    }
    public DiscountBuilder WithCreatedAt(DateTime created)
    {
        _createdAt = created; return this;
    }

    public Result<Discount> Build()
    {
        return Discount.Create(
            _code,
            _discountType,
            _fixedAmount,
            _percentage,
            _maxUses,
            _minOrderAmount,
            _maxUsesPerUser,
            _validFrom,
            _validTo,
            _autoApply
        );
    }

    public Result<Discount> BuildFrom()
    {
        return Discount.From(
            _id ?? Guid.NewGuid(),
            _code,
            _discountType,
            _fixedAmount,
            _percentage,
            _maxUses,
            _uses,
            _minOrderAmount,
            _maxUsesPerUser,
            _validFrom,
            _validTo,
            _isActive,
            _autoApply,
            _createdAt
        );
    }
}