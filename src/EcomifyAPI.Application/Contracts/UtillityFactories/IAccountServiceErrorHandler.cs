using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Response;

namespace EcomifyAPI.Application.Contracts.UtillityFactories;

public interface IAccountServiceErrorHandler
{
    Task HandleRegistrationFailureAsync(UserResponseDTO user, string? profileImagePath);
    Task HandleUnexpectedRegistrationExceptionAsync(string userEmail, string? profileImagePath);
    Task HandleUnexpectedUpdateExceptionAsync(ApplicationUserMapping user);
}