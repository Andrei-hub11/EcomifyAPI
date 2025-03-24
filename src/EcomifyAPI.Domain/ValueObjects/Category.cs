using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Domain.ValueObjects;

public sealed class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public Category(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public static Result<Category> Create(Guid id, string name, string description)
    {
        var errors = ValidateCategory(id, name, description);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new Category(id, name, description);
    }

    private static ReadOnlyCollection<ValidationError> ValidateCategory(Guid id, string name, string description)
    {
        var errors = new List<ValidationError>();

        if (id == Guid.Empty)
        {
            errors.Add(ValidationError.Create("Id is required", "ERR_CATEGORY_ID_REQUIRED", "Id"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(ValidationError.Create("Name is required", "ERR_CATEGORY_NAME_REQUIRED", "Name"));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add(ValidationError.Create("Description is required", "ERR_CATEGORY_DESCRIPTION_REQUIRED", "Description"));
        }

        return errors.AsReadOnly();
    }

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required", nameof(description));
        }

        Name = name;
        Description = description;
    }


}