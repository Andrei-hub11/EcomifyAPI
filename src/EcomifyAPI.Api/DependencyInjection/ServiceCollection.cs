using System.Threading.RateLimiting;

using EcomifyAPI.Api.DependencyInjection;
using EcomifyAPI.Api.Middleware;

using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace EcomifyAPI.Api.DependencyInjection;

public static class ServiceCollection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddCors(configuration);

        services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddSingleton<ResultFilter>();

        services.AddProblemDetails();
        services.AddExceptionHandler<ExceptionHandler>();

        services.AddFixedWindowRateLimiting(environment);

        return services;
    }

    private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        //var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        //if (allowedOrigins == null)
        //{
        //    throw new ArgumentNullException(nameof(allowedOrigins), "'AllowedOrigins' cannot be null");
        //}

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin",
            builder => builder
                              .WithMethods("GET", "POST", "PUT", "DELETE")
                              .AllowAnyHeader()
                              .AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddFixedWindowRateLimiting(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddRateLimiter(options =>
        {
            if (environment.IsEnvironment("INTEGRATION_TEST"))
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
               httpContext =>
               {
                   // Get a unique identifier for each test request
                   var testId = httpContext.Request.Headers["X-Test-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();

                   return RateLimitPartition.GetFixedWindowLimiter(
                       testId,
                       factory: partition => new FixedWindowRateLimiterOptions
                       {
                           Window = TimeSpan.FromSeconds(2), // Window smaller for tests
                           PermitLimit = 3,                  // Limit smaller for tests
                           QueueLimit = 0,
                           QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                       });
               });
            }
            else
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                  httpContext => RateLimitPartition.GetFixedWindowLimiter(
                      httpContext.Connection.RemoteIpAddress?.ToString() ?? "Default",
                      factory: partition => new FixedWindowRateLimiterOptions
                      {
                          Window = TimeSpan.FromSeconds(10),
                          PermitLimit = 5,
                          QueueLimit = 2,
                          QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                      }));

            }

            /* options.AddPolicy("FixedWindow",
            httpContext => RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "Default",
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromSeconds(10),
                    PermitLimit = 5,
                    QueueLimit = 2,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                })); */



            options.OnRejected = async (context, token) =>
            {
                var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                var problemDetails = factory.CreateProblemDetails(
                    context.HttpContext,
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Too many requests",
                    detail: "You have made too many requests. Please try again later.",
                    type: "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/429"
                    );

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
            };
        });

        return services;
    }
}