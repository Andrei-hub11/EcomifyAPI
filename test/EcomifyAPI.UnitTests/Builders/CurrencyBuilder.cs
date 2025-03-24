using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.ValueObjects.Builders;

public class CurrencyBuilder
{
    private string _code = "USD";
    private decimal _amount = 100.00m;

    public CurrencyBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public CurrencyBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public Currency Build()
    {
        return new Currency(_code, _amount);
    }
}