using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class MoneyBuilder
{
    private string _code = "USD";
    private decimal _amount = 100.00m;

    public MoneyBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public MoneyBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public Money Build()
    {
        return new Money(_code, _amount);
    }
}