using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Domain.ValueObjects;

namespace EcomifyAPI.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string KeycloakId { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public Email Email { get; private set; }
    public ProfileImagePath ProfileImagePath { get; private set; }
    public IReadOnlySet<string> Roles { get; private set; }

    private User(Guid id, string keycloakId, string name, string email, string profileImagePath, IReadOnlySet<string> roles)
    {
        Id = id;
        KeycloakId = keycloakId;
        UserName = name;
        Email = new Email(email);
        ProfileImagePath = new ProfileImagePath(profileImagePath);
        Roles = roles;
    }

    public static Result<User> Create(Guid id, string keycloakId, string name, string email,
        string profileImagePath, IReadOnlySet<string> roles)
    {
        var errors = ValidateUser(id, keycloakId, name, email, roles);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new User(id, keycloakId, name, email, profileImagePath, roles);
    }

    public static Result<User> From(ApplicationUserMapping applicationUser)
    {
        var errors = ValidateUser(applicationUser.Id, applicationUser.KeycloakId, applicationUser.UserName, applicationUser.Email,
            applicationUser.Roles.ToHashSet());

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new User(applicationUser.Id, applicationUser.KeycloakId, applicationUser.UserName, applicationUser.Email,
            applicationUser.ProfileImagePath, applicationUser.Roles.ToHashSet());
    }

    private static ReadOnlyCollection<ValidationError> ValidateUser(Guid id, string keycloakId, string name, string email,
         IReadOnlySet<string> roles)
    {
        var errors = new List<ValidationError>();
        var isValidRole = new HashSet<string> { "Admin", "User", "Manager" };

        if (id == Guid.Empty)
        {
            errors.Add(ValidationError.Create("Id cannot be empty", "ERR_ID_EMPTY", "Id"));
        }

        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            errors.Add(ValidationError.Create("KeycloakId cannot be empty", "ERR_KEYCLOAK_ID_EMPTY", "KeycloakId"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(ValidationError.Create("UserName cannot be empty", "ERR_USERNAME_EMPTY", "UserName"));
        }

        if (name.Length > 120)
        {
            errors.Add(ValidationError.Create("UserName cannot be longer than 120 characters", "ERR_USERNAME_TOO_LONG", "UserName"));
        }

        // Validate Roles
        if (!roles.Any())
        {
            errors.Add(ValidationError.Create("User must have at least one role", "ERR_ROLES_EMPTY", "Roles"));
        }

        if (!roles.All(role => isValidRole.Contains(role)))
        {
            errors.Add(ValidationError.Create("Invalid role specified", "ERR_INVALID_ROLE", "Roles"));
        }

        return errors.AsReadOnly();
    }

    private static ReadOnlyCollection<ValidationError> ValidateProfileUpdate(string? newUsername, string? newProfileImagePath)
    {
        var errors = new List<ValidationError>();

        if (newUsername is not null)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                errors.Add(ValidationError.Create("UserName cannot be empty", "ERR_USERNAME_EMPTY", "UserName"));
            }
            else if (newUsername.Length > 120)
            {
                errors.Add(ValidationError.Create("UserName cannot be longer than 120 characters", "ERR_USERNAME_TOO_LONG", "UserName"));
            }
        }

        /*         if (newProfileImagePath is not null && string.IsNullOrWhiteSpace(newProfileImagePath))
                {
                    if (string.IsNullOrWhiteSpace(newProfileImagePath))
                    {
                        errors.Add(ValidationError.Create("Profile image path cannot be empty", "ERR_PROFILE_IMAGE_PATH_EMPTY", "ProfileImagePath"));
                    }
                } */

        return errors.AsReadOnly();
    }

    public Result<bool> UpdateProfile(string? newUsername = null, string? newEmail = null,
        string? newProfileImagePath = null)
    {
        var errors = ValidateProfileUpdate(newUsername, newProfileImagePath);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        UserName = newUsername ?? UserName;
        Email = string.IsNullOrWhiteSpace(newEmail) ? Email : new Email(newEmail);
        ProfileImagePath = string.IsNullOrWhiteSpace(newProfileImagePath) ? ProfileImagePath : new ProfileImagePath(newProfileImagePath);

        return true;
    }
}