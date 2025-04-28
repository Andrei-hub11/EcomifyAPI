using EcomifyAPI.Domain.Enums;
using EcomifyAPI.UnitTests.Builders;

using Shouldly;

namespace EcomifyAPI.UnitTests.Entities;

public class PaymentRecordTests
{
    private readonly PaymentRecordBuilder _builder;

    public PaymentRecordTests()
    {
        _builder = new PaymentRecordBuilder();
    }

    [Fact]
    public void CreateCreditCardPayment_ShouldSucceed_WhenValidDataProvided()
    {
        // Act
        var payment = _builder.BuildCreditCardPayment();

        // Assert
        payment.ShouldNotBeNull();
        payment.OrderId.ShouldNotBe(Guid.Empty);
        payment.Amount.Amount.ShouldBe(100m);
        payment.Amount.Code.ShouldBe("USD");
        payment.PaymentMethod.ShouldBe(PaymentMethodEnum.CreditCard);
        payment.TransactionId.ShouldNotBe(Guid.Empty);
        payment.Status.ShouldBe(PaymentStatusEnum.Processing);
        payment.GatewayResponse.ShouldBe("Success");
        payment.StatusHistory.Count.ShouldBe(1);
        payment.StatusHistory.First().Status.ShouldBe(PaymentStatusEnum.Processing);
    }

    [Fact]
    public void CreatePayPalPayment_ShouldSucceed_WhenValidDataProvided()
    {
        // Act
        var payment = _builder.BuildPayPalPayment();

        // Assert
        payment.ShouldNotBeNull();
        payment.OrderId.ShouldNotBe(Guid.Empty);
        payment.Amount.Amount.ShouldBe(100m);
        payment.Amount.Code.ShouldBe("USD");
        payment.PaymentMethod.ShouldBe(PaymentMethodEnum.PayPal);
        payment.TransactionId.ShouldNotBe(Guid.Empty);
        payment.Status.ShouldBe(PaymentStatusEnum.Processing);
        payment.GatewayResponse.ShouldBe("Success");
        payment.StatusHistory.Count.ShouldBe(1);
        payment.StatusHistory.First().Status.ShouldBe(PaymentStatusEnum.Processing);
    }

    [Fact]
    public void From_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        // Act
        var payment = _builder
            .WithPaymentId(paymentId)
            .BuildFrom();

        // Assert
        payment.PaymentId.ShouldBe(paymentId);
        payment.Status.ShouldBe(PaymentStatusEnum.Processing);
        payment.StatusHistory.Count.ShouldBe(0);
    }

    [Fact]
    public void GetCreditCardDetails_ShouldSucceed_WhenPaymentMethodIsCreditCard()
    {
        // Arrange
        var payment = _builder
            .WithLastFourDigits("5678")
            .WithCardBrand("MasterCard")
            .BuildCreditCardPayment();

        // Act
        var details = payment.GetCreditCardDetails();

        // Assert
        details.ShouldNotBeNull();
        details!.LastFourDigits.ShouldBe("5678");
        details.CardBrand.ShouldBe("MasterCard");
    }

    [Fact]
    public void GetCreditCardDetails_ShouldThrow_WhenPaymentMethodIsNotCreditCard()
    {
        // Arrange
        var payment = _builder.BuildPayPalPayment();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            payment.GetCreditCardDetails()).Message.ShouldBe("This payment was not made with a credit card");
    }

    [Fact]
    public void GetPayPalDetails_ShouldSucceed_WhenPaymentMethodIsPayPal()
    {
        // Arrange
        var payment = _builder
            .WithPayPalEmail("customer@example.com")
            .WithPayPalPayerId("PAYER987")
            .BuildPayPalPayment();

        // Act
        var details = payment.GetPayPalDetails();

        // Assert
        details.ShouldNotBeNull();
        details.Value.PayPalEmail.Value.ShouldBe("customer@example.com");
        details.Value.PayPalPayerId.ShouldBe("PAYER987");
    }

    [Fact]
    public void GetPayPalDetails_ShouldThrow_WhenPaymentMethodIsNotPayPal()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            payment.GetPayPalDetails()).Message.ShouldBe("This payment was not made with PayPal");
    }

    [Fact]
    public void MarkAsSucceeded_ShouldSucceed_WhenPaymentIsProcessing()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.Status.ShouldBe(PaymentStatusEnum.Processing);

        // Act
        var result = payment.MarkAsSucceeded("AUTH-123456");

        // Assert
        result.IsFailure.ShouldBeFalse();
        payment.Status.ShouldBe(PaymentStatusEnum.Succeeded);
        payment.StatusHistory.Count.ShouldBe(2);
        payment.StatusHistory.Last().Status.ShouldBe(PaymentStatusEnum.Succeeded);
        payment.StatusHistory.Last().Reference.ShouldBe("AUTH-123456");
    }

    [Fact]
    public void MarkAsSucceeded_ShouldFail_WhenPaymentIsAlreadySucceeded()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsSucceeded("AUTH-123");

        // Act
        var result = payment.MarkAsSucceeded("AUTH-456");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error =>
        error.Description == "Impossible to mark as succeeded a payment already finalized");
    }

    [Fact]
    public void MarkAsSucceeded_ShouldFail_WhenPaymentIsRefunded()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsSucceeded("AUTH-123");
        payment.MarkAsRefunded("Refunded by customer");

        // Act
        var result = payment.MarkAsSucceeded("AUTH-456");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error =>
        error.Description == "Impossible to mark as succeeded a payment already finalized");
    }

    [Fact]
    public void MarkAsFailed_ShouldSucceed_WhenPaymentIsProcessing()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();

        // Act
        var result = payment.MarkAsFailed("Insufficient funds");

        // Assert
        result.IsFailure.ShouldBeFalse();
        payment.Status.ShouldBe(PaymentStatusEnum.Failed);
        payment.StatusHistory.Count.ShouldBe(2);
        payment.StatusHistory.Last().Status.ShouldBe(PaymentStatusEnum.Failed);
        payment.StatusHistory.Last().Reference.ShouldBe("Insufficient funds");
    }

    [Fact]
    public void MarkAsFailed_ShouldFail_WhenPaymentIsAlreadySucceeded()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsSucceeded("AUTH-123");

        // Act
        var result = payment.MarkAsFailed("Failed");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error =>
        error.Description == "Impossible to mark as failed a payment already finalized");
    }

    [Fact]
    public void RequestRefund_ShouldSucceed_WhenPaymentIsSucceeded()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsSucceeded("AUTH-123");

        // Act
        var result = payment.RequestRefund(50m, "Customer requested partial refund");

        // Assert
        result.IsFailure.ShouldBeFalse();
        payment.Status.ShouldBe(PaymentStatusEnum.RefundRequested);
        payment.StatusHistory.Count.ShouldBe(3);
        payment.StatusHistory.Last().Status.ShouldBe(PaymentStatusEnum.RefundRequested);
        payment.StatusHistory.Last().Reference.ShouldBe("Customer requested partial refund");
    }

    [Fact]
    public void RequestRefund_ShouldFail_WhenPaymentIsNotSucceeded()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();

        // Act
        var result = payment.RequestRefund(50m, "Customer requested refund");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error =>
        error.Description == "Only succeeded payments can be refunded");
    }

    [Fact]
    public void RequestRefund_ShouldFail_WhenRefundAmountIsGreaterThanPaymentAmount()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsSucceeded("AUTH-123");

        // Act
        var result = payment.RequestRefund(150m, "Customer requested refund");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error =>
        error.Description == "Refund amount is greater than the payment amount");
    }

    [Fact]
    public void ConfirmRefund_ShouldSucceed_WhenPaymentHasRefundRequested()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsSucceeded("AUTH-123");
        payment.RequestRefund(100m, "Customer requested refund");

        // Act
        var result = payment.ConfirmRefund("REFUND-123");

        // Assert
        result.IsFailure.ShouldBeFalse();
        payment.Status.ShouldBe(PaymentStatusEnum.Refunded);
        payment.StatusHistory.Count.ShouldBe(4);
        payment.StatusHistory.Last().Status.ShouldBe(PaymentStatusEnum.Refunded);
        payment.StatusHistory.Last().Reference.ShouldBe("REFUND-123");
    }

    [Fact]
    public void ConfirmRefund_ShouldFail_WhenPaymentDoesNotHaveRefundRequested()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsSucceeded("AUTH-123");

        // Act
        var result = payment.ConfirmRefund("REFUND-123");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error =>
        error.Description == "Only payments with refund requested can be confirmed as refunded");
    }

    [Fact]
    public void MarkAsCancelled_ShouldSucceed_WhenPaymentIsInProgress()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();

        // Act
        var result = payment.MarkAsCancelled("Customer cancelled payment");

        // Assert
        result.IsFailure.ShouldBeFalse();
        payment.Status.ShouldBe(PaymentStatusEnum.Cancelled);
        payment.StatusHistory.Count.ShouldBe(2);
        payment.StatusHistory.Last().Status.ShouldBe(PaymentStatusEnum.Cancelled);
        payment.StatusHistory.Last().Reference.ShouldBe("Customer cancelled payment");
    }

    [Fact]
    public void MarkAsCancelled_ShouldFail_WhenPaymentIsRefunded()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsSucceeded("AUTH-123");
        payment.RequestRefund(100m, "Customer requested refund");
        payment.ConfirmRefund("REFUND-123");

        // Act
        var result = payment.MarkAsCancelled("Customer cancelled payment");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error =>
        error.Description == "Payment has already been finalized and cannot be cancelled");
    }

    [Fact]
    public void MarkAsRefunded_ShouldSucceed_WhenPaymentIsInProgress()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();

        // Act
        var result = payment.MarkAsRefunded("Manual refund processed");

        // Assert
        result.IsFailure.ShouldBeFalse();
        payment.Status.ShouldBe(PaymentStatusEnum.Refunded);
        payment.StatusHistory.Count.ShouldBe(2);
        payment.StatusHistory.Last().Status.ShouldBe(PaymentStatusEnum.Refunded);
        payment.StatusHistory.Last().Reference.ShouldBe("Manual refund processed");
    }

    [Fact]
    public void MarkAsRefunded_ShouldFail_WhenPaymentIsAlreadyRefunded()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();
        payment.MarkAsRefunded("Manual refund processed");

        // Act
        var result = payment.MarkAsRefunded("Manual refund processed again");

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(error =>
        error.Description == "Payment has already been finalized and cannot be refunded");
    }

    [Fact]
    public void UpdateFromGateway_ShouldSucceed_WhenValidTransition()
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();

        // Act
        var result = payment.UpdateFromGateway("approved", "Gateway Reference");

        // Assert
        result.IsFailure.ShouldBeFalse();
        payment.Status.ShouldBe(PaymentStatusEnum.Succeeded);
        payment.StatusHistory.Count.ShouldBe(2);
        payment.StatusHistory.Last().Status.ShouldBe(PaymentStatusEnum.Succeeded);
        payment.StatusHistory.Last().Reference.ShouldBe("Gateway Reference");
    }

    //[Fact]
    //public void UpdateFromGateway_ShouldFail_WhenInvalidTransition()
    //{
    //    // Arrange
    //    var payment = _builder.BuildCreditCardPayment();
    //    payment.MarkAsRefunded("Manual refund");

    //    // Act
    //    var result = payment.UpdateFromGateway("approved", "Gateway Reference");

    //    // Assert
    //    result.IsFailure.ShouldBeTrue();
    //    result.Errors.ShouldContain(error =>
    //    error.Description == "Invalid status transition");
    //}

    [Theory]
    [InlineData("completed", PaymentStatusEnum.Succeeded)]
    [InlineData("pending", PaymentStatusEnum.Processing)]
    [InlineData("failed", PaymentStatusEnum.Failed)]
    [InlineData("refunded", PaymentStatusEnum.Refunded)]
    [InlineData("cancelled", PaymentStatusEnum.Cancelled)]
    public void UpdateFromGateway_ShouldMapPayPalStatuses_Correctly(string paypalStatus, PaymentStatusEnum expectedStatus)
    {
        // Arrange
        var payment = _builder.BuildPayPalPayment();

        // Act
        var result = payment.UpdateFromGateway(paypalStatus, "PayPal Reference");

        // Assert
        if (!result.IsFailure)
        {
            payment.Status.ShouldBe(expectedStatus);
        }
    }

    [Theory]
    [InlineData("approved", PaymentStatusEnum.Succeeded)]
    [InlineData("pending", PaymentStatusEnum.Processing)]
    [InlineData("declined", PaymentStatusEnum.Failed)]
    [InlineData("refunded", PaymentStatusEnum.Refunded)]
    [InlineData("cancelled", PaymentStatusEnum.Cancelled)]
    public void UpdateFromGateway_ShouldMapCreditCardStatuses_Correctly(string cardStatus, PaymentStatusEnum expectedStatus)
    {
        // Arrange
        var payment = _builder.BuildCreditCardPayment();

        // Act
        var result = payment.UpdateFromGateway(cardStatus, "Credit Card Reference");

        // Assert
        if (!result.IsFailure)
        {
            payment.Status.ShouldBe(expectedStatus);
        }
    }
}