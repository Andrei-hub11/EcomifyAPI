namespace EcomifyAPI.Application.Contracts.Contexts;

public interface IUserContext
{
    bool IsAuthenticated { get; }
    string UserId { get; }
    bool IsAdmin { get; }
    string Email { get; }
}