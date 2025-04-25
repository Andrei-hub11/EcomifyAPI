using EcomifyAPI.Domain.Enums;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.UnitTests.Builders;

public class PaymentStatusChangeBuilder
{
    private Guid _id = Guid.NewGuid();
    private PaymentStatusEnum _status = PaymentStatusEnum.Processing;
    private DateTime _timestamp = DateTime.UtcNow;
    private string _reference = "REF123456";

    public PaymentStatusChangeBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PaymentStatusChangeBuilder WithStatus(PaymentStatusEnum status)
    {
        _status = status;
        return this;
    }

    public PaymentStatusChangeBuilder WithTimestamp(DateTime timestamp)
    {
        _timestamp = timestamp;
        return this;
    }

    public PaymentStatusChangeBuilder WithReference(string reference)
    {
        _reference = reference;
        return this;
    }

    public PaymentStatusChange Build()
    {
        return new PaymentStatusChange(_id, _status, _timestamp, _reference);
    }
}