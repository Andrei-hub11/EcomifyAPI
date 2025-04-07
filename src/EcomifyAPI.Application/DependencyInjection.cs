using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Application.Contracts.TokenJWT;
using EcomifyAPI.Application.Services.Account;
using EcomifyAPI.Application.Services.Carts;
using EcomifyAPI.Application.Services.Cookies;
using EcomifyAPI.Application.Services.Images;
using EcomifyAPI.Application.Services.ImageServiceConfiguration;
using EcomifyAPI.Application.Services.Keycloak;
using EcomifyAPI.Application.Services.Orders;
using EcomifyAPI.Application.Services.Payment;
using EcomifyAPI.Application.Services.Products;
using EcomifyAPI.Application.Services.TokenJWT;

using Microsoft.Extensions.DependencyInjection;

namespace EcomifyAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ICookieService, CookieService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();

        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentMethodFactory, PaymentMethodFactory>();
        services.AddScoped<IPaymentMethod, CreditCardPaymentService>();
        services.AddScoped<IPaymentMethod, PayPalPaymentService>();

        services.AddScoped<IKeycloakService, KeycloakService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IImagesService, ImageService>();

        services.AddSingleton<IImageServiceConfiguration>(new ImageServiceConfiguration(AppDomain.CurrentDomain.BaseDirectory));
        return services;
    }
}