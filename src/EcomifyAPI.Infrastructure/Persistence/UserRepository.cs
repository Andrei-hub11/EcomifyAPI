using System.Data;

using Dapper;

using EcomifyAPI.Application.Contracts.Repositories;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.Entities;

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
        ue.username AS UserName,
        COALESCE(ARRAY_AGG(DISTINCT r.name) FILTER (WHERE r.name IS NOT NULL), '{}') AS Roles
    FROM users u
    JOIN user_entity ue ON u.keycloak_id = ue.id
    LEFT JOIN user_group_membership ugm ON ue.id = ugm.user_id
    LEFT JOIN keycloak_group g ON ugm.group_id = g.id
    LEFT JOIN group_role_mapping grm ON g.id = grm.group_id
    LEFT JOIN keycloak_role r ON grm.role_id = r.id
    WHERE u.keycloak_id = @Id
    GROUP BY u.id, ue.id;";

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
        ue.username AS UserName,
        COALESCE(ARRAY_AGG(DISTINCT r.name) FILTER (WHERE r.name IS NOT NULL), '{}') AS Roles
    FROM users u
    JOIN user_entity ue ON u.keycloak_id = ue.id
    LEFT JOIN user_group_membership ugm ON ue.id = ugm.user_id
    LEFT JOIN keycloak_group g ON ugm.group_id = g.id
    LEFT JOIN group_role_mapping grm ON g.id = grm.group_id
    LEFT JOIN keycloak_role r ON grm.role_id = r.id
    WHERE u.email = @Email
    GROUP BY u.id, ue.id;";

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
        cancellationToken.ThrowIfCancellationRequested();

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
            query,
            new { Username = userId },
            transaction: Transaction
        );

        return [.. result];
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

    public async Task<bool> CreateApplicationUser(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string query =
            @"INSERT INTO users (keycloak_id, email, profile_picture_url) VALUES (@KeycloakId, @Email, @ProfilePictureUrl)";

        int result = await Connection.ExecuteAsync(
            query,
            new
            {
                user.KeycloakId,
                Email = user.Email.Value,
                ProfilePictureUrl = user.ProfileImagePath.Value,
            },
            transaction: Transaction
        );

        return result > 0;
    }

    public async Task AddRolesToUser(
        string userId,
        IReadOnlySet<string> roles,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string query =
            @"INSERT INTO ApplicationUserRoles (UserId, Name) VALUES (@UserId, @Name)";

        foreach (var item in roles)
        {
            await Connection.ExecuteAsync(
                query,
                new { UserId = userId, Name = item },
                transaction: Transaction
            );
        }
    }

    public async Task<bool> UpdateApplicationUser(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string query =
            @"UPDATE users SET email = @Email,
        profile_picture_url = @ProfileImagePath
        WHERE keycloak_id = @KeycloakId";

        int result = await Connection.ExecuteAsync(
            query,
            new
            {
                user.Email,
                user.ProfileImagePath,
            },
            transaction: Transaction
        );

        return result > 0;
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