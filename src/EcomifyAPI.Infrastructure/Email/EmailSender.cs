﻿using EcomifyAPI.Application.Contracts.Email;
using EcomifyAPI.Common.Helpers;
using EcomifyAPI.Contracts.EmailModels;
using EcomifyAPI.Domain.ValueObjects;

using FluentEmail.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EcomifyAPI.Infrastructure.Email;

internal class EmailSender : IEmailSender
{
    private readonly IFluentEmail _fluentEmail;
    private readonly IWebHostEnvironment _environment;
    private readonly string _srcDirectory;

    public EmailSender(IFluentEmail fluentEmail, IWebHostEnvironment environment)
    {
        _fluentEmail = fluentEmail
            ?? throw new ArgumentNullException(nameof(fluentEmail));
        _environment = environment;
        _srcDirectory = DirectoryHelper.FindDirectoryAbove(AppDomain.CurrentDomain.BaseDirectory, "src");
    }

    public async Task Send(EmailMetadata emailMetadata)
    {
        if (_environment.IsEnvironment("INTEGRATION_TEST"))
        {
            return;
        }

        await _fluentEmail.To(emailMetadata.ToAddress)
            .Subject(emailMetadata.Subject)
            .Body(emailMetadata.Body)
            .SendAsync();
    }

    public async Task SendPasswordResetEmail(string toAddress, string resetLink, TimeSpan tokenValidity)
    {
        if (_environment.IsEnvironment("INTEGRATION_TEST"))
        {
            return;
        }

        var model = new PasswordResetEmail(resetLink, tokenValidity);

        string templatePath = Path.Combine(_srcDirectory, "EcomifyAPI.Infrastructure", "Email", "Templates", "PasswordReset.cshtml");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found at: {templatePath}");
        }

        await _fluentEmail
            .To(toAddress)
            .Subject("Password Reset")
            .UsingTemplateFromFile(templatePath, model)
            .SendAsync();
    }

    public async Task SendPaymentCancellationEmail(string toAddress, OrderDetails orderDetails)
    {
        if (_environment.IsEnvironment("INTEGRATION_TEST"))
        {
            return;
        }

        var model = new PaymentCancellationEmail(
            orderDetails.OrderId.ToString(),
            orderDetails.TotalAmount,
            orderDetails.Amount.Code,
            orderDetails.CustomerName,
            "Payment cancelled by user request"
        );

        string templatePath = Path.Combine(_srcDirectory, "EcomifyAPI.Infrastructure", "Email", "Templates", "PaymentCancellation.cshtml");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found at: {templatePath}");
        }

        await _fluentEmail
            .To(toAddress)
            .Subject("Order Cancellation Confirmation")
            .UsingTemplateFromFile(templatePath, model)
            .SendAsync();
    }

    public async Task SendPaymentRefundEmail(string toAddress, OrderDetails orderDetails)
    {
        if (_environment.IsEnvironment("INTEGRATION_TEST"))
        {
            return;
        }

        var model = new PaymentRefundEmail(
            orderDetails.OrderId.ToString(),
            orderDetails.TotalAmount,
            orderDetails.Amount.Code,
            orderDetails.CustomerName,
            "Payment refunded by administrator",
            DateTime.UtcNow.AddDays(5) // Estimated refund date (5 business days from now)
        );

        string templatePath = Path.Combine(_srcDirectory, "EcomifyAPI.Infrastructure", "Email", "Templates", "PaymentRefund.cshtml");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found at: {templatePath}");
        }

        await _fluentEmail
            .To(toAddress)
            .Subject("Payment Refund Confirmation")
            .UsingTemplateFromFile(templatePath, model)
            .SendAsync();
    }

    public async Task SendOrderConfirmationEmail(string toAddress, OrderConfirmationEmail orderDetails)
    {
        if (_environment.IsEnvironment("INTEGRATION_TEST"))
        {
            return;
        }

        string templatePath = Path.Combine(_srcDirectory, "EcomifyAPI.Infrastructure", "Email", "Templates", "OrderConfirmation.cshtml");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found at: {templatePath}");
        }

        await _fluentEmail
            .To(toAddress)
            .Subject("Your Order Confirmation")
            .UsingTemplateFromFile(templatePath, orderDetails)
            .SendAsync();
    }

    public async Task SendDeliveryConfirmationEmail(string toAddress, DeliveryConfirmationEmail email, CancellationToken cancellationToken = default)
    {
        if (_environment.IsEnvironment("INTEGRATION_TEST"))
        {
            return;
        }

        string templatePath = Path.Combine(_srcDirectory, "EcomifyAPI.Infrastructure", "Email", "Templates", "DeliveryConfirmation.cshtml");

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found at: {templatePath}");
        }

        await _fluentEmail
            .To(toAddress)
            .Subject("Delivery Confirmation")
            .UsingTemplateFromFile(templatePath, email)
            .SendAsync();
    }
}