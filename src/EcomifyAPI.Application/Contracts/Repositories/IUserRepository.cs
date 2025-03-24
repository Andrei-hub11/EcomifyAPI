using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IUserRepository : IRepository
{
    Task<ApplicationUserMapping?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken
    );
    Task<ApplicationUserMapping?> GetUserByEmailAsync(
        string userEmail,
        CancellationToken cancellationToken
    );
    Task<bool> CreateApplicationUser(User user, CancellationToken cancellationToken);
    Task AddRolesToUser(
        string userId,
        IReadOnlySet<string> roles,
        CancellationToken cancellationToken
    );
    Task<bool> UpdateApplicationUser(User user, CancellationToken cancellationToken);
    /*  Task<IEnumerable<ApplicationUserMapping>> GetTestUsersAsync(
         CancellationToken cancellationToken
     ); */
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
}