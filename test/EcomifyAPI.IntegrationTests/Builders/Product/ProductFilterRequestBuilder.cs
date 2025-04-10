using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class ProductFilterRequestBuilder
{
    private int _stockThreshold = 10;
    private int _pageSize = 10;
    private int _pageNumber = 1;
    private string? _name = null;
    private ProductStatusDTO? _status = null;
    private string? _category = null;

    public ProductFilterRequestBuilder WithStockThreshold(int stockThreshold)
    {
        _stockThreshold = stockThreshold;
        return this;
    }

    public ProductFilterRequestBuilder WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public ProductFilterRequestBuilder WithPageNumber(int pageNumber)
    {
        _pageNumber = pageNumber;
        return this;
    }

    public ProductFilterRequestBuilder WithName(string? name)
    {
        _name = name;
        return this;
    }

    public ProductFilterRequestBuilder WithStatus(ProductStatusDTO? status)
    {
        _status = status;
        return this;
    }

    public ProductFilterRequestBuilder WithCategory(string? category)
    {
        _category = category;
        return this;
    }

    public ProductFilterRequestDTO Build()
    {
        return new ProductFilterRequestDTO(
            _stockThreshold,
            _pageSize,
            _pageNumber,
            _name,
            _status,
            _category
        );
    }
}