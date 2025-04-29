using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class PaymentFilterRequestBuilder
{
    private int _pageSize = 10;
    private int _pageNumber = 1;
    private string? _customerId = null;
    private decimal? _amount = null;
    private PaymentStatusDTO? _status = null;
    private PaymentMethodEnumDTO? _paymentMethod = null;
    private string? _paymentReference = null;
    private DateTime? _startDate = null;
    private DateTime? _endDate = null;

    public PaymentFilterRequestBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public PaymentFilterRequestBuilder WithPageNumber(int pageNumber)
    {
        _pageNumber = pageNumber;
        return this;
    }

    public PaymentFilterRequestBuilder WithCustomerId(string customerId)
    {
        _customerId = customerId;
        return this;
    }

    public PaymentFilterRequestBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public PaymentFilterRequestBuilder WithStatus(PaymentStatusDTO status)
    {
        _status = status;
        return this;
    }

    public PaymentFilterRequestBuilder WithPaymentMethod(PaymentMethodEnumDTO paymentMethod)
    {
        _paymentMethod = paymentMethod;
        return this;
    }

    public PaymentFilterRequestBuilder WithPaymentReference(string paymentReference)
    {
        _paymentReference = paymentReference;
        return this;
    }

    public PaymentFilterRequestBuilder WithStartDate(DateTime startDate)
    {
        _startDate = startDate;
        return this;
    }

    public PaymentFilterRequestBuilder WithEndDate(DateTime endDate)
    {
        _endDate = endDate;
        return this;
    }

    public PaymentFilterRequestDTO Build()
    {
        return new PaymentFilterRequestDTO(
            _pageSize,
            _pageNumber,
            _customerId,
            _amount,
            _status,
            _paymentMethod,
            _paymentReference,
            _startDate,
            _endDate
        );
    }
}