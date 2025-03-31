using EcomifyAPI.Contracts.DapperModels;

namespace EcomifyAPI.Application.Contracts.UtillityFactories;

public interface IAccountServiceErrorHandler
{
    Task HandleUnexpectedAuthenticationExceptionAsync(string userEmail, string? profileImagePath);
    Task HandleUnexpectedUpdateExceptionAsync(ApplicationUserMapping user);
}