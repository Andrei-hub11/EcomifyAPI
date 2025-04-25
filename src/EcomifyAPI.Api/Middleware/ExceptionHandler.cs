using System.Net;

using EcomifyAPI.Api.Models;
using EcomifyAPI.Api.Utils;
using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Common.Extensions;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Domain.Exceptions;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EcomifyAPI.Api.Middleware;

public class ExceptionHandler : IExceptionHandler
{
    private readonly ILoggerHelper<ExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandler(ILoggerHelper<ExceptionHandler> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        // Log the full details for debugging
        _logger.LogError(ExceptionDetailsHelper.GetExceptionDetails(exception, httpContext));

        var result = GetProblemDetails(httpContext, exception);
        httpContext.Response.StatusCode = result.Status ?? 500;

        if (result is CustomProblemDetails customProblemDetails)
        {
            await httpContext.Response.WriteAsJsonAsync(customProblemDetails, cancellationToken: cancellationToken);
            return true;
        }

        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken: cancellationToken);
        return true;
    }

    private ProblemDetails GetProblemDetails(HttpContext context, Exception exception) =>
        exception switch
        {
            BadRequestException => CreateProblemDetails(
                context,
                HttpStatusCode.BadRequest,
                "Invalid Request",
                GetSafeErrorMessage(exception)
            ),

            UnauthorizedAccessException => CreateProblemDetails(
                context,
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                "You are not authorized to perform this action."
            ),

            DomainException domainException => CreateValidationErrorDetails(
                context,
                HttpStatusCode.UnprocessableEntity,
                "Validation Error",
                "One or more validation errors occurred.",
                domainException.Errors
            ),

            _ => CreateProblemDetails(
                context,
                HttpStatusCode.InternalServerError,
                "Server Error",
                "An unexpected error occurred. Please try again later."
            ),
        };

    private string GetSafeErrorMessage(Exception exception)
    {
        // Only return detailed errors in development
        if (_environment.IsDevelopment())
        {
            return exception.Message;
        }

        return exception switch
        {
            BadRequestException =>
                "The request was invalid. Please check your input and try again.",
            _ => "An error occurred while processing your request.",
        };
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail
    )
    {
        var statusCodeValue = (int)statusCode;

        return new ProblemDetails
        {
            Status = statusCodeValue,
            Type = $"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{statusCodeValue}",
            Title = title,
            Detail = detail,
            Instance = $"{context.Request.Method} {context.Request.Path}",
        };
    }

    private CustomProblemDetails CreateValidationErrorDetails(
        HttpContext context,
        HttpStatusCode statusCode,
        string title,
        string detail,
        IReadOnlyList<IError> errors
    )
    {
        var statusCodeValue = (int)statusCode;

        Dictionary<string, ValidationErrorDetail[]> validationErrors = errors
            .OfType<ValidationError>()
            .GroupBy(error => error.Field)
            .ToDictionary(
                group => group.Key.CapitalizeFirstLetter(),
                group => group.Select(error =>
                new ValidationErrorDetail(error.Code, error.Description)).ToArray());

        var validationErrorDetails = new CustomProblemDetails(validationErrors)
        {
            Status = statusCodeValue,
            Type = $"https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/{statusCodeValue}",
            Title = title,
            Detail = detail,
            Instance = $"{context.Request.Method} {context.Request.Path}",
        };

        return validationErrorDetails;
    }
}