using EcomifyAPI.Api.DependencyInjection;
using EcomifyAPI.Api.Middleware;

namespace EcomifyAPI.Api.DependencyInjection;

public static class ServiceCollection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(configuration);

        services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddSingleton<ResultFilter>();

        services.AddProblemDetails();
        services.AddExceptionHandler<ExceptionHandler>();
        services.AddSignalR();

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
}