using EcomifyAPI.Contracts.Request;

namespace EcomifyAPI.IntegrationTests.Builders;

public class CategoryRequestBuilder
{
    private string _name = $"Categoria {Guid.NewGuid()}";
    private string _description = "Descrição padrão de categoria";

    public CategoryRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CategoryRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CreateCategoryRequestDTO Build()
    {
        return new CreateCategoryRequestDTO(_name, _description);
    }
}