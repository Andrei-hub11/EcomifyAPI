using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private IDbConnection? _connection = null;
    private IDbTransaction? _transaction = null;

    private IDbConnection Connection =>
        _connection ?? throw new InvalidOperationException("Connection has not been initialized.");
    private IDbTransaction Transaction =>
        _transaction
        ?? throw new InvalidOperationException("Transaction has not been initialized.");

    public void Initialize(IDbConnection connection, IDbTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    /* public async Task<ApplicationUserMapping?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        const string query =
            @"SELECT u.Id, u.UserName, u.Email, u.ProfileImage,
        u.ProfileImagePath, ur.Name as RoleName
        FROM ApplicationUsers u
        LEFT JOIN ApplicationUserRoles ur ON u.Id = ur.UserId
        WHERE u.Id = @Id";

        var userDictionary = new Dictionary<string, ApplicationUserMapping>();

        var result = await Connection.QueryAsync<
            ApplicationUserMapping,
            string,
            ApplicationUserMapping
        >(
            new CommandDefinition(
                query,
                new { Id = userId },
                cancellationToken: cancellationToken,
                transaction: Transaction
            ),
            (user, role) =>
            {
                if (!userDictionary.TryGetValue(user.Id, out var userEntry))
                {
                    userEntry = user;
                    userEntry.Roles = new HashSet<string>();
                    userDictionary.Add(userEntry.Id, userEntry);
                }

                if (role != null)
                {
                    ((HashSet<string>)userEntry.Roles).Add(role);
                }

                return userEntry;
            },
            splitOn: "RoleName"
        );

        return userDictionary.Values.FirstOrDefault();
    } */

    public async Task<ApplicationUserMapping?> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken
    )
    {
        const string query = @"SELECT 
        u.id AS Id,
        u.keycloak_id AS KeycloakId,
        u.email AS Email,
        u.profile_picture_url AS ProfileImagePath,
        ua.value AS UserName,
        COALESCE(ARRAY_AGG(DISTINCT r.name) FILTER (WHERE r.name IN ('Admin', 'User')), '{}') AS Roles
    FROM users u
    JOIN user_entity ue ON u.keycloak_id = ue.id
    LEFT JOIN user_attribute ua ON ue.id = ua.user_id AND ua.name = 'normalizedUserName'
    LEFT JOIN user_group_membership ugm ON ue.id = ugm.user_id
    LEFT JOIN keycloak_group g ON ugm.group_id = g.id
    LEFT JOIN group_role_mapping grm ON g.id = grm.group_id
    LEFT JOIN keycloak_role r ON grm.role_id = r.id
    WHERE u.keycloak_id = @Id
    GROUP BY u.id, ue.id, ua.value;";

        var result = await Connection.QueryAsync<ApplicationUserMapping>(
            new CommandDefinition(
                query,
                new { Id = userId },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );

        return result.FirstOrDefault();
    }

    public async Task<ApplicationUserMapping?> GetUserByEmailAsync(
        string userEmail,
        CancellationToken cancellationToken
    )
    {
        const string query = @"
    SELECT 
        u.id AS Id,
        u.keycloak_id AS KeycloakId,
        u.email AS Email,
        u.profile_picture_url AS ProfileImagePath,
        ua.value AS UserName,
        COALESCE(ARRAY_AGG(DISTINCT r.name) FILTER (WHERE r.name IN ('Admin', 'User')), '{}') AS Roles
    FROM users u
    JOIN user_entity ue ON u.keycloak_id = ue.id
    LEFT JOIN user_attribute ua ON ue.id = ua.user_id AND ua.name = 'normalizedUserName'
    LEFT JOIN user_group_membership ugm ON ue.id = ugm.user_id
    LEFT JOIN keycloak_group g ON ugm.group_id = g.id
    LEFT JOIN group_role_mapping grm ON g.id = grm.group_id
    LEFT JOIN keycloak_role r ON grm.role_id = r.id
    WHERE u.email = @Email
    GROUP BY u.id, ue.id, ua.value;";

        var result = await Connection.QueryAsync<ApplicationUserMapping>(
            new CommandDefinition(
                query,
                new { Email = userEmail },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );

        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<UserRoleMapping>> GetUserRolesAsync(string userId, CancellationToken cancellationToken)
    {
        const string query = @"SELECT DISTINCT 
                    r.name AS role_name
                FROM user_entity u
                JOIN user_group_membership ugm ON u.id = ugm.user_id
                JOIN keycloak_group g ON ugm.group_id = g.id
                JOIN group_role_mapping grm ON g.id = grm.group_id
                JOIN keycloak_role r ON grm.role_id = r.id
                LEFT JOIN client c ON r.client = c.id
                WHERE u.username = @Username
                AND r.name IN ('Admin', 'User', 'Monitor')";

        var result = await Connection.QueryAsync<UserRoleMapping>(
            new CommandDefinition(
                query,
                new { Username = userId },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );

        return [.. result];
    }

    public async Task<UserAddressMapping?> GetUserAddressByFieldsAsync(string userId, string street, int number, string city,
    string state, string zipCode, string country, string complement, CancellationToken cancellationToken)
    {

        const string query = @"SELECT * FROM user_addresses WHERE user_keycloak_id = @UserId 
        AND street = @Street AND number = @Number AND city = @City AND state = @State AND zip_code = @ZipCode 
        AND country = @Country AND complement = @Complement";

        var result = await Connection.QueryAsync<UserAddressMapping>(
            new CommandDefinition(
                query,
                new
                {
                    UserId = userId,
                    Street = street,
                    Number = number,
                    City = city,
                    State = state,
                    ZipCode = zipCode,
                    Country = country,
                    Complement = complement
                },
                cancellationToken: cancellationToken,
                transaction: Transaction
            )
        );

        return result.FirstOrDefault();
    }

    /*     public async Task<IEnumerable<ApplicationUserMapping>> GetTestUsersAsync(
        CancellationToken cancellationToken
    )
        {
            cancellationToken.ThrowIfCancellationRequested();

            const string query =
                @"
                SELECT u.Id, u.UserName, u.Email, u.ProfileImage, u.ProfileImagePath, ur.Name as RoleName
                FROM ApplicationUsers u
                LEFT JOIN ApplicationUserRoles ur ON u.Id = ur.UserId
                WHERE u.Email LIKE '%@test.com' OR u.Email LIKE '%@example.com'";

            var userDictionary = new Dictionary<string, ApplicationUserMapping>();

            await Connection.QueryAsync<ApplicationUserMapping, string, ApplicationUserMapping>(
                query,
                (user, roleName) =>
                {
                    if (!userDictionary.TryGetValue(user.Id, out var existingUser))
                    {
                        existingUser = user;
                        existingUser.Roles = new HashSet<string>();
                        userDictionary.Add(user.Id, existingUser);
                    }

                    if (roleName != null)
                    {
                        ((HashSet<string>)existingUser.Roles).Add(roleName);
                    }

                    return existingUser;
                },
                splitOn: "RoleName",
                transaction: Transaction
            );

            return userDictionary.Values;
        } */

    public async Task CreateApplicationUser(User user, CancellationToken cancellationToken)
    {
        const string query =
            @"INSERT INTO users (keycloak_id, email, profile_picture_url) VALUES (@KeycloakId, @Email, @ProfilePictureUrl) RETURNING id";

        await Connection.ExecuteAsync(new CommandDefinition(
            query,
            new
            {
                user.KeycloakId,
                Email = user.Email.Value,
                ProfilePictureUrl = user.ProfileImagePath.Value,
            },
            cancellationToken: cancellationToken,
            transaction: Transaction
        ));
    }

    public async Task<Guid> CreateUserAddress(Address address, string userKeycloakId, CancellationToken cancellationToken)
    {
        const string query = @"INSERT INTO user_addresses (user_keycloak_id, street, number, city, state, 
        zip_code, country, complement) VALUES (@UserKeycloakId, @Street, @Number, @City, 
        @State, @ZipCode, @Country, @Complement) RETURNING id";

        var result = await Connection.ExecuteScalarAsync<Guid>(new CommandDefinition(
            query,
            new
            {
                UserKeycloakId = userKeycloakId,
                address.Street,
                address.Number,
                address.City,
                address.State,
                address.ZipCode,
                address.Country,
                address.Complement
            },
            cancellationToken: cancellationToken,
            transaction: Transaction
        ));

        return result;
    }

    public async Task UpdateApplicationUser(User user, CancellationToken cancellationToken)
    {
        const string query =
            @"UPDATE users SET
            profile_picture_url = @ProfileImagePath
        WHERE keycloak_id = @KeycloakId";

        await Connection.ExecuteAsync(new CommandDefinition(
            query,
            new
            {
                ProfileImagePath = user.ProfileImagePath.Value,
                user.KeycloakId
            },
            cancellationToken: cancellationToken,
            transaction: Transaction
        ));
    }

    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string deleteRolesQuery = "DELETE FROM ApplicationUserRoles WHERE UserId = @UserId";
        const string deleteUserQuery = "DELETE FROM ApplicationUsers WHERE Id = @UserId";

        await Connection.ExecuteAsync(
            deleteRolesQuery,
            new { UserId = userId },
            transaction: Transaction
        );
        await Connection.ExecuteAsync(
            deleteUserQuery,
            new { UserId = userId },
            transaction: Transaction
        );
    }
}