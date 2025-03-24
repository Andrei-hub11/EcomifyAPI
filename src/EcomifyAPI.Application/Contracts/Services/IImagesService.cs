using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IImagesService
{
    Task<ProfileImage> GetProfileImageAsync(string profileImage);
    Task DeleteProfileImageAsync(string relativeImagePath);
}