using EcomifyAPI.Domain.Enums;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.ValueObjects;

public class PaymentStatusChangeTests
{
    private readonly PaymentStatusChangeBuilder _builder;

    public PaymentStatusChangeTests()
    {
        _builder = new PaymentStatusChangeBuilder();
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange & Act
        var statusChange = _builder
            .WithStatus(PaymentStatusEnum.Succeeded)
            .WithReference("REF123456")
            .Build();

        // Assert
        statusChange.Id.ShouldNotBe(Guid.Empty);
        statusChange.Status.ShouldBe(PaymentStatusEnum.Succeeded);
        statusChange.Reference.ShouldBe("REF123456");
        statusChange.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Create_ShouldPreserveAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var status = PaymentStatusEnum.RefundRequested;
        var timestamp = new DateTime(2023, 5, 10, 14, 30, 0, DateTimeKind.Utc);
        var reference = "REFUND-12345";

        // Act
        var statusChange = _builder
            .WithId(id)
            .WithStatus(status)
            .WithTimestamp(timestamp)
            .WithReference(reference)
            .Build();

        // Assert
        statusChange.Id.ShouldBe(id);
        statusChange.Status.ShouldBe(status);
        statusChange.Timestamp.ShouldBe(timestamp);
        statusChange.Reference.ShouldBe(reference);
    }

    [Fact]
    public void Create_ShouldHandleEmptyReference()
    {
        // Arrange & Act
        var statusChange = _builder
            .WithReference(string.Empty)
            .Build();

        // Assert
        statusChange.Reference.ShouldBe(string.Empty);
    }

    [Fact]
    public void Create_ShouldHandleAllPaymentStatuses()
    {
        // Testing each enum value works
        foreach (PaymentStatusEnum status in Enum.GetValues(typeof(PaymentStatusEnum)))
        {
            // Arrange & Act
            var statusChange = _builder
                .WithStatus(status)
                .Build();

            // Assert
            statusChange.Status.ShouldBe(status);
        }
    }
}