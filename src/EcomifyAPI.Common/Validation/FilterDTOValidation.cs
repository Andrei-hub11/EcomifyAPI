using EcomifyAPI.Common.Utils.ResultError;

namespace EcomifyAPI.Common.Validation;

public static class FilterDTOValidation
{
    public static IReadOnlyList<ValidationError> Validate(
     int stockThreshold,
     int pageSize,
     int pageNumber,
     string? name,
     string? category)
    {
        var errors = new List<ValidationError>();

        if (pageSize <= 0)
        {
            errors.Add(Error.Validation("Page size must be greater than 0", "ERR_GT_0", "PageSize"));
        }

        if (pageNumber <= 0)
        {
            errors.Add(Error.Validation("Page number must be greater than 0", "ERR_GT_0", "PageNumber"));
        }

        if (stockThreshold <= 0)
        {
            errors.Add(Error.Validation("Stock threshold must be greater than 0", "ERR_GT_0", "StockThreshold"));
        }

        if (name != null && name.Length > 100)
        {
            errors.Add(Error.Validation("Name must not be greater than 100 characters", "ERR_NAME_GT_100", "Name"));
        }

        if (category != null && category.Length > 100)
        {
            errors.Add(Error.Validation("Category must not be greater than 100 characters", "ERR_CATEGORY_GT_100", "Category"));
        }

        return errors;
    }
}