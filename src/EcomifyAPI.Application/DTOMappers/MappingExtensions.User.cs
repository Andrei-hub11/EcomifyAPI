﻿using EcomifyAPI.Contracts.DapperModels;
using EcomifyAPI.Contracts.Models;
using EcomifyAPI.Contracts.Response;
using EcomifyAPI.Domain.Entities;

namespace EcomifyAPI.Application.DTOMappers;

public static class MappingExtensionsUser
{
    public static UserResponseDTO ToResponseDTO(this UserMapping user)
    {
        return new UserResponseDTO(user.Id, user.UserName, user.Email, user.ProfileImagePath);
    }

    public static UserResponseDTO ToResponseDTO(this User user)
    {
        return new UserResponseDTO(user.KeycloakId, user.UserName, user.Email.Value, user.ProfileImagePath.Value);
    }

    public static UserResponseDTO ToResponseDTO(this UserInfoMapping user)
    {
        return new UserResponseDTO(user.Id, user.UserName, user.Email, user.ProfileImagePath);
    }

    public static UserResponseDTO ToResponseDTO(this ApplicationUserMapping user)
    {
        return new UserResponseDTO(user.KeycloakId, user.UserName, user.Email, user.ProfileImagePath);
    }

    public static IReadOnlyList<UserResponseDTO> ToReponseDTO(this IEnumerable<UserMapping> users)
    {
        return [.. users.Select(user => user.ToResponseDTO())];
    }
}