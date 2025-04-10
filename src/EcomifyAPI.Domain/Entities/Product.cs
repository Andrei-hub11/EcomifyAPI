namespace EcomifyAPI.Domain.Entities;

using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.Exceptions;
using EcomifyAPI.Domain.ValueObjects;

public sealed class Product
{
    private readonly List<ProductCategory> _productCategories = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; }
    public int Stock { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public ProductStatusEnum Status { get; private set; }
    public IReadOnlyList<ProductCategory> ProductCategories => _productCategories.AsReadOnly();

    private Product(Guid id, string name, string description, Money price, int stock, string imageUrl, ProductStatusEnum status)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        ImageUrl = imageUrl;
        Status = status;
    }

    public static Result<Product> Create(
        string name,
        string description,
        decimal price,
        string currencyCode,
        int stock,
        string imageUrl,
        ProductStatusEnum status,
        Guid? id = null
        )
    {
        var errors = ValidateProduct(name, description, price, stock, imageUrl, status, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Product(id ?? Guid.Empty, name, description, new Money(currencyCode, price), stock, imageUrl, status);
    }

    public static Result<Product> From(
        Guid id,
        string name,
        string description,
        decimal price,
        string currencyCode,
        int stock,
        string imageUrl,
        ProductStatusEnum status)
    {
        var errors = ValidateProduct(name, description, price, stock, imageUrl, status, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Product(id, name, description, new Money(currencyCode, price), stock, imageUrl, status);
    }

    private static ReadOnlyCollection<ValidationError> ValidateProduct(
        string name,
        string description,
        decimal price,
        int stock,
        string imageUrl,
        ProductStatusEnum status,
        Guid? id = null
        )
    {
        var errors = new List<ValidationError>();

        if (id is not null && id == Guid.Empty)
        {
            errors.Add(ValidationError.Create("Id is required", "ERR_ID_REQ", "Id"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(ValidationError.Create("Name is required", "ERR_NAME_REQ", "Name"));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add(ValidationError.Create("Description is required", "ERR_DESC_REQ", "Description"));
        }

        if (price <= 0)
        {
            errors.Add(ValidationError.Create("Price must be greater than 0", "ERR_PRICE_GT0", "Price"));
        }

        if (stock < 0)
        {
            errors.Add(ValidationError.Create("Stock must be greater than 0", "ERR_STOCK_GT0", "Stock"));
        }

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            errors.Add(ValidationError.Create("ImageUrl is required", "ERR_IMG_REQ", "ImageUrl"));
        }

        if (status == ProductStatusEnum.Inactive)
        {
            errors.Add(ValidationError.Create("Status must be active", "ERR_STATUS_ACTIVE", "Status"));
        }

        return errors.AsReadOnly();
    }

    public bool DecrementStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new DomainException(Error.Validation("Quantity must be greater than 0", "ERR_QUANTITY_GT0", "Quantity"));
        }

        if (Stock - quantity < 0)
        {
            return false;
        }

        if (Stock == quantity)
        {
            return false;
        }

        Stock -= quantity;

        return true;
    }

    public bool UpdateStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new DomainException(Error.Validation("Quantity must be greater than 0", "ERR_QUANTITY_GT0", "Quantity"));
        }

        if (Stock == quantity)
        {
            return false;
        }

        Stock = quantity;

        return true;
    }

    public bool UpdatePrice(decimal price, string currencyCode)
    {
        if (price <= 0)
        {
            throw new DomainException(Error.Validation("Price must be greater than 0", "ERR_PRICE_GT0", "Price"));
        }

        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new DomainException(Error.Validation("CurrencyCode is required", "ERR_CUR_REQ", "CurrencyCode"));
        }

        if (Price.Amount == price)
        {
            return false;
        }

        Price = new Money(currencyCode, price);

        return true;
    }

    public bool UpdateStatus(ProductStatusEnum status)
    {
        if (Status == status)
        {
            return false;
        }

        Status = status;

        return true;
    }

    public bool UpdateImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            throw new DomainException(Error.Validation("ImageUrl is required", "ERR_IMG_REQ", "ImageUrl"));
        }

        if (ImageUrl == imageUrl)
        {
            return false;
        }

        ImageUrl = imageUrl;

        return true;
    }

    public bool UpdateCategories(List<ProductCategory> categories)
    {
        if (categories.Count == 0)
        {
            throw new DomainException(Error.Validation("Categories must be at least one", "ERR_CAT_MIN1", "Categories"));
        }

        if (categories.GroupBy(c => c.CategoryId).Any(g => g.Count() > 1))
        {
            throw new DomainException(Error.Validation("Categories must be unique", "ERR_CAT_UNQ", "Categories"));
        }

        if (categories.Any(c => c.ProductId != Id))
        {
            throw new DomainException(Error.Validation("Categories must be associated with the product", "ERR_CAT_ASSOC", "Categories"));
        }

        _productCategories.Clear();
        _productCategories.AddRange(categories);

        return true;
    }

    public bool UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(Error.Validation("Name is required", "ERR_NAME_REQ", "Name"));
        }

        if (Name == name)
        {
            return false;
        }

        Name = name;

        return true;
    }

    public bool UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException(Error.Validation("Description is required", "ERR_DESC_REQ", "Description"));
        }

        if (Description == description)
        {
            return false;
        }

        Description = description;

        return true;
    }
}