using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class DiscountFilterRequestBuilder
{
    private int _pageSize = 10;
    private int _pageNumber = 1;
    private string? _code = null;
    private string? _customerId = null;
    private Guid? _categoryId = null;
    private Guid? _productId = null;
    private bool? _status = null;
    private DiscountTypeEnum? _type = null;
    private decimal? _minOrderAmount = null;
    private decimal? _maxOrderAmount = null;
    private int? _minUses = null;
    private int? _maxUses = null;
    private int? _minUsesPerUser = null;
    private int? _maxUsesPerUser = null;
    private DateTime? _validFrom = null;
    private DateTime? _validTo = null;
    private bool? _isActive = null;
    private bool? _autoApply = null;

    public DiscountFilterRequestBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public DiscountFilterRequestBuilder WithPageNumber(int pageNumber)
    {
        _pageNumber = pageNumber;
        return this;
    }

    public DiscountFilterRequestBuilder WithCode(string? code)
    {
        _code = code;
        return this;
    }

    public DiscountFilterRequestBuilder WithCustomerId(string? customerId)
    {
        _customerId = customerId;
        return this;
    }

    public DiscountFilterRequestBuilder WithCategoryId(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public DiscountFilterRequestBuilder WithProductId(Guid? productId)
    {
        _productId = productId;
        return this;
    }

    public DiscountFilterRequestBuilder WithStatus(bool? status)
    {
        _status = status;
        return this;
    }

    public DiscountFilterRequestBuilder WithType(DiscountTypeEnum? type)
    {
        _type = type;
        return this;
    }

    public DiscountFilterRequestBuilder WithMinOrderAmount(decimal? minOrderAmount)
    {
        _minOrderAmount = minOrderAmount;
        return this;
    }

    public DiscountFilterRequestBuilder WithMaxOrderAmount(decimal? maxOrderAmount)
    {
        _maxOrderAmount = maxOrderAmount;
        return this;
    }

    public DiscountFilterRequestBuilder WithMinUses(int? minUses)
    {
        _minUses = minUses;
        return this;
    }

    public DiscountFilterRequestBuilder WithMaxUses(int? maxUses)
    {
        _maxUses = maxUses;
        return this;
    }

    public DiscountFilterRequestBuilder WithMinUsesPerUser(int? minUsesPerUser)
    {
        _minUsesPerUser = minUsesPerUser;
        return this;
    }

    public DiscountFilterRequestBuilder WithMaxUsesPerUser(int? maxUsesPerUser)
    {
        _maxUsesPerUser = maxUsesPerUser;
        return this;
    }

    public DiscountFilterRequestBuilder WithValidFrom(DateTime? validFrom)
    {
        _validFrom = validFrom;
        return this;
    }

    public DiscountFilterRequestBuilder WithValidTo(DateTime? validTo)
    {
        _validTo = validTo;
        return this;
    }

    public DiscountFilterRequestBuilder WithIsActive(bool? isActive)
    {
        _isActive = isActive;
        return this;
    }

    public DiscountFilterRequestBuilder WithAutoApply(bool? autoApply)
    {
        _autoApply = autoApply;
        return this;
    }

    public DiscountFilterRequestDTO Build()
    {
        return new DiscountFilterRequestDTO(
            _pageSize,
            _pageNumber,
            _code,
            _customerId,
            _categoryId,
            _productId,
            _status,
            _type,
            _minOrderAmount,
            _maxOrderAmount,
            _minUses,
            _maxUses,
            _minUsesPerUser,
            _maxUsesPerUser,
            _validFrom,
            _validTo,
            _isActive,
            _autoApply
        );
    }
}