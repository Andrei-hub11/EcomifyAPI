using EcomifyAPI.Api.Utils;
using EcomifyAPI.Application.Contracts.Logging;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcomifyAPI.Api.Middleware;

public class ResultFilter : IAsyncResultFilter
{
    private readonly ILoggerHelper<ResultFilter> _logger;

    public ResultFilter(ILoggerHelper<ResultFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var result = context.Result;

        if (result is ObjectResult objectResult && objectResult.Value is ProblemDetails problemDetails)
        {
            _logger.LogError(ExceptionDetailsHelper.GetProblemDetails(problemDetails, context.HttpContext));
        }

        if (result is BadRequestObjectResult badRequestResult)
        {
            _logger.LogError(ExceptionDetailsHelper.GetBadRequestDetails(badRequestResult,
                context.HttpContext));
        }
    }
}