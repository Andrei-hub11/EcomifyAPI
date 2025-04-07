using EcomifyAPI.Contracts.Request;

public class AddItemRequestBuilder
{
    private Guid _productId = Guid.NewGuid();
    private int _quantity = 1;

    public AddItemRequestBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public AddItemRequestBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public AddItemRequestDTO Build() => new(_productId, _quantity);
}

public class UpdateItemQuantityRequestBuilder
{
    private Guid _productId = Guid.NewGuid();
    private int _quantity = 1;

    public UpdateItemQuantityRequestBuilder WithProductId(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public UpdateItemQuantityRequestBuilder WithQuantity(int quantity)
    {
        _quantity = quantity;
        return this;
    }

    public UpdateItemQuantityRequestDTO Build() => new(_productId, _quantity);
}