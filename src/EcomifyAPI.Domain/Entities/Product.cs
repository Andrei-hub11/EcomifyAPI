namespace EcomifyAPI.Domain.Entities;

using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Contracts.Enums;
using EcomifyAPI.Domain.ValueObjects;

public sealed class Product
{
    private readonly List<ProductCategory> _categories = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Currency Price { get; private set; }
    public int Stock { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public ProductStatusEnum Status { get; private set; }
    public IReadOnlyList<ProductCategory> Categories => _categories.AsReadOnly();

    private Product(Guid id, string name, string description, Currency price, int stock, string imageUrl, ProductStatusEnum status)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        ImageUrl = imageUrl;
        Status = status;
    }

    public static Result<Product> Create(Guid id, string name, string description, decimal price, int stock, string imageUrl, ProductStatusEnum status)
    {
        var errors = ValidateProduct(id, name, description, price, stock, imageUrl, status);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Product(id, name, description, new Currency("BRL", price), stock, imageUrl, status);
    }

    private static ReadOnlyCollection<ValidationError> ValidateProduct(Guid id, string name, string description, decimal price, int stock, string imageUrl, ProductStatusEnum status)
    {
        var errors = new List<ValidationError>();

        if (id == Guid.Empty)
        {
            errors.Add(ValidationError.Create("Id is required", "ERR_ID_REQUIRED", "Id"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(ValidationError.Create("Name is required", "ERR_NAME_REQUIRED", "Name"));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add(ValidationError.Create("Description is required", "ERR_DESCRIPTION_REQUIRED", "Description"));
        }

        if (price <= 0)
        {
            errors.Add(ValidationError.Create("Price must be greater than 0", "ERR_PRICE_MUST_BE_GREATER_THAN_0", "Price"));
        }

        if (stock < 0)
        {
            errors.Add(ValidationError.Create("Stock must be greater than 0", "ERR_STOCK_MUST_BE_GREATER_THAN_0", "Stock"));
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            errors.Add(ValidationError.Create("ImageUrl is required", "ERR_IMAGE_URL_REQUIRED", "ImageUrl"));
        }

        if (status == ProductStatusEnum.Inactive)
        {
            errors.Add(ValidationError.Create("Status must be active", "ERR_STATUS_MUST_BE_ACTIVE", "Status"));
        }

        return errors.AsReadOnly();
    }

    public bool DecrementStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Quantity must be greater than 0");
        }

        if (Stock - quantity < 0)
        {
            return false;
        }

        Stock -= quantity;

        return true;
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Quantity must be greater than 0");
        }

        Stock = quantity;
    }

    public void UpdatePrice(decimal price, string currencyCode)
    {
        if (price <= 0)
        {
            throw new ArgumentException("Price must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException("CurrencyCode is required");
        }

        Price = new Currency(currencyCode, price);
    }

    public void UpdateStatus(ProductStatusEnum status)
    {
        Status = status;
    }

    public void UpdateImageUrl(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            throw new ArgumentException("ImageUrl is required");
        }
    }

    public void UpdateCategories(List<ProductCategory> categories)
    {
        if (categories.Count == 0)
        {
            throw new ArgumentException("Categories must be at least one");
        }

        if (Categories.Any(c => categories.Any(c2 => c2.CategoryId == c.CategoryId)))
        {
            throw new ArgumentException("Categories must be unique");
        }

        _categories.AddRange(categories);
    }

}