﻿using System.Collections.ObjectModel;

using EcomifyAPI.Common.Utils.Result;
using EcomifyAPI.Common.Utils.ResultError;
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

    public static Result<User> Create(string keycloakId, string name, string email,
        string profileImagePath, IReadOnlySet<string> roles, Guid? id = null)
    {
        var errors = ValidateUser(keycloakId, name, email, roles, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new User(id ?? Guid.Empty, keycloakId, name, email, profileImagePath, roles);
    }

    public static Result<User> From(Guid id, string keycloakId, string name, string email,
        string profileImagePath, IReadOnlySet<string> roles)
    {
        var errors = ValidateUser(keycloakId, name, email, roles, id);

        if (errors.Count != 0)
        {
            return Result.Fail(errors);
        }

        return new User(id, keycloakId, name, email, profileImagePath, roles);
    }

    private static ReadOnlyCollection<ValidationError> ValidateUser(string keycloakId, string name, string email,
        IReadOnlySet<string> roles, Guid? id = null)
    {
        var errors = new List<ValidationError>();
        var isValidRole = new HashSet<string> { "Admin", "User", "Manager" };

        if (id is not null && id == Guid.Empty)
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

        if (name.Contains(' '))
        {
            errors.Add(ValidationError.Create("UserName cannot contain spaces", "ERR_SPACES_IN_USERNAME", "UserName"));
        }

        if (name.Length < 3)
        {
            errors.Add(ValidationError.Create("UserName must be at least 3 characters long", "ERR_USERNAME_TOO_SHORT", "UserName"));
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

            if (newUsername.Contains(' '))
            {
                errors.Add(ValidationError.Create("UserName cannot contain spaces", "ERR_SPACES_IN_USERNAME", "UserName"));
            }

            if (newUsername.Length < 3)
            {
                errors.Add(ValidationError.Create("UserName must be at least 3 characters long", "ERR_USERNAME_TOO_SHORT", "UserName"));
            }

            if (newUsername.Length > 120)
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

        UserName = string.IsNullOrWhiteSpace(newUsername) ? UserName : newUsername;
        /*  Email = string.IsNullOrWhiteSpace(newEmail) ? Email : new Email(newEmail); */
        ProfileImagePath = string.IsNullOrWhiteSpace(newProfileImagePath) ? ProfileImagePath : new ProfileImagePath(newProfileImagePath);

        return true;
    }
}