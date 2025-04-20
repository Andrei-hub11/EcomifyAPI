using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Application.Contracts.Services;

public interface IImagesService
{
    Task<byte[]> GetProfileImageBytesAsync(string profileImagePath);
    Task<ProfileImage> GetProfileImageAsync(string profileImage);
    Task DeleteProfileImageAsync(string relativeImagePath);
}