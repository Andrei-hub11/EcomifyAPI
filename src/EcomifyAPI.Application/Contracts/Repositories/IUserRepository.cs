using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Application.Contracts.Repositories;

public interface IUserRepository : IRepository
{
    Task<ApplicationUserMapping?> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken
    );
    Task<ApplicationUserMapping?> GetUserByEmailAsync(
        string userEmail,
        CancellationToken cancellationToken
    );
    Task<UserAddressMapping?> GetUserAddressByFieldsAsync(
        string userId,
        string street,
        int number,
        string city,
        string state,
        string zipCode,
        string country,
        string complement,
        CancellationToken cancellationToken
    );
    Task CreateApplicationUser(User user, CancellationToken cancellationToken);
    Task<Guid> CreateUserAddress(Address address, string userKeycloakId, CancellationToken cancellationToken);
    Task UpdateApplicationUser(User user, CancellationToken cancellationToken);
    /*  Task<IEnumerable<ApplicationUserMapping>> GetTestUsersAsync(
         CancellationToken cancellationToken
     ); */
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken);
}