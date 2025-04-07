using EcomifyAPI.Application.Contracts.Contexts;
using EcomifyAPI.Application.Contracts.Data;
using EcomifyAPI.Application.Contracts.Email;

using EcomifyAPI.Application.Contracts.Logging;
using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Application.Contracts.UtillityFactories;
using EcomifyAPI.Infrastructure;
using EcomifyAPI.Infrastructure.Contexts;
using EcomifyAPI.Infrastructure.Data;
using EcomifyAPI.Infrastructure.Email;
using EcomifyAPI.Infrastructure.Extensions;
using EcomifyAPI.Infrastructure.Logging;
using EcomifyAPI.Infrastructure.Persistence;
using EcomifyAPI.Infrastructure.Security;
using EcomifyAPI.Infrastructure.UtillityFactories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Serilog;
using Serilog.Filters;

namespace EcomifyAPI.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddHttpContextAccessor()
            .AddPersistence()
            .AddFluentEmail(configuration)
            .AddSerilog(configuration)
            .AddHttpClient()
            .AddKeycloakAuthentication(configuration)
            .AddKeycloakPolicy()
            .AddFactories();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<DapperContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IUserContext, UserContexts>();

        return services;
    }

    public static IServiceCollection AddFluentEmail(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var emailSettings = configuration.GetSection("Smtp");

        var defaultFromEmail = emailSettings["DefaultFromEmail"];
        var host = emailSettings["Host"];
        var port = emailSettings.GetValue<int>("Port");
        var userName = emailSettings["UserName"];
        var password = emailSettings["Password"];

        services
            .AddFluentEmail(defaultFromEmail)
            .AddRazorRenderer()
            .AddSmtpSender(host, port, userName, password);

        return services;
    }

    private static IServiceCollection AddSerilog(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Filter.ByExcluding(
                Matching.FromSource("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware")
            )
            .CreateLogger();

        services.AddSingleton(typeof(ILoggerHelper<>), typeof(LoggerHelper<>));

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });

        return services;
    }

    private static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<KeycloakSettings>(configuration.GetSection("Keycloak"));

        // Register KeycloakTokenValidationConfiguration
        services.AddSingleton<
            IConfigureOptions<JwtBearerOptions>,
            KeycloakTokenValidationConfiguration
        >();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        return services;
    }

    private static IServiceCollection AddKeycloakPolicy(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                "Admin",
                policy => policy.RequireAssertion(context => context.User.HasRole("Admin"))
            );

        return services;
    }

    private static IServiceCollection AddFactories(this IServiceCollection services)
    {
        services.AddScoped<IAccountServiceErrorHandler, AccountServiceErrorHandler>();
        services.AddScoped<IKeycloakServiceErrorHandler, KeycloakServiceErrorHandler>();

        return services;
    }
}