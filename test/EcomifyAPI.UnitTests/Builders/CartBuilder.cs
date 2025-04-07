using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class CartBuilder
{
    private Guid? _id = Guid.NewGuid();
    private string _userId = "user123";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime? _updatedAt = null;


    public CartBuilder WithId(Guid? id)
    {
        _id = id;
        return this;
    }

    public CartBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public CartBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public CartBuilder WithUpdatedAt(DateTime? updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }




    public Result<Cart> Build()
    {
        return Cart.Create(_userId);
    }

    public Result<Cart> BuildFrom(List<CartItem> items)
    {
        return Cart.From(_id ?? Guid.NewGuid(), _userId, _createdAt, _updatedAt, items);
    }
}