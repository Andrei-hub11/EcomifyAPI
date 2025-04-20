using EcomifyAPI.Application.Contracts.Services;
using EcomifyAPI.Common.Helpers;
using EcomifyAPI.Contracts.Models;

namespace EcomifyAPI.Application.Services.Images;

public class ImageService : IImagesService
{
    private static string API_ROOT_DIRECTORY = string.Empty;

    public ImageService(IImageServiceConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.ApiRootDirectory))
        {
            throw new InvalidOperationException("API_ROOT_DIRECTORY was not initialized correctly.");
        }

        API_ROOT_DIRECTORY = configuration.ApiRootDirectory;
    }

    public async Task<byte[]> GetProfileImageBytesAsync(string profileImagePath)
    {
        if (string.IsNullOrWhiteSpace(profileImagePath))
        {
            return [];
        }

        if (!File.Exists(profileImagePath))
        {
            return [];
        }

        return await File.ReadAllBytesAsync(profileImagePath);
    }

    public async Task<ProfileImage> GetProfileImageAsync(string profileImage)
    {
        var result = new ProfileImage();
        if (string.IsNullOrWhiteSpace(profileImage))
        {
            return result;
        }

        var fileName = GenerateFileName();
        var profileImageBytes = ConvertFromBase64String(profileImage);
        var filePath = GetProfileImagePath(fileName);

        await SaveImageAsync(filePath, profileImageBytes);
        var profileImagePath = GetRelativeProfileImagePath(fileName);

        result.ProfileImageBytes = profileImageBytes;
        result.ProfileImagePath = profileImagePath;
        return result;
    }

    private static async Task SaveImageAsync(string filePath, byte[] imageBytes)
    {
        try
        {
            await File.WriteAllBytesAsync(filePath, imageBytes);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving the image: {ex.Message}");
        }
    }

    public async Task DeleteProfileImageAsync(string relativeImagePath)
    {
        if (string.IsNullOrWhiteSpace(relativeImagePath))
        {
            throw new ArgumentException("Image path cannot be null or empty.", nameof(relativeImagePath));
        }

        var filePath = Path.Combine(API_ROOT_DIRECTORY, "wwwroot", relativeImagePath);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                await Task.CompletedTask;
            }
            else
            {
                throw new FileNotFoundException("The file does not exist.", filePath);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting the image: {ex.Message}");
        }
    }

    private static string GenerateFileName()
    {
        return Guid.NewGuid().ToString() + ".jpg";
    }

    private static byte[] ConvertFromBase64String(string base64String)
    {
        return Base64Helper.ConvertFromBase64String(base64String);
    }

    private static string GetProfileImagePath(string fileName)
    {
        return Path.Combine(API_ROOT_DIRECTORY, "wwwroot", "users", "images", fileName);
    }

    private static string GetRelativeProfileImagePath(string fileName)
    {
        return NormalizePath(Path.Combine("users", "images", fileName));
    }

    private static string NormalizePath(string path)
    {
        return path.Replace("\\", "/");
    }
}