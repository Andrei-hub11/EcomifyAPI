using EcomifyAPI.Application.Contracts.Services;

namespace EcomifyAPI.Application.Services.ImageServiceConfiguration;

public class ImageServiceConfiguration : IImageServiceConfiguration
{
    public string ApiRootDirectory { get; }
    private const string API_FOLDER_NAME = "EcomifyAPI.Api";
    private const string SLN_EXTENSION = ".sln";

    public ImageServiceConfiguration(string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
            throw new ArgumentNullException(nameof(baseDirectory));

        ApiRootDirectory = FindApiRootDirectory(baseDirectory);
    }

    private static string FindApiRootDirectory(string baseDirectory)
    {
        try
        {
            // Normalize the path and ensure it exists
            var normalizedPath = Path.GetFullPath(baseDirectory);
            if (!Directory.Exists(normalizedPath))
                throw new DirectoryNotFoundException(
                    $"Base directory does not exist: {normalizedPath}"
                );

            // First find the solution root (directory containing .sln file)
            var solutionRoot = FindSolutionRoot(normalizedPath);

            // Look for the API project directly in the solution directory structure
            var apiDirectory = Directory
                .GetDirectories(solutionRoot, API_FOLDER_NAME, SearchOption.AllDirectories)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(apiDirectory))
                throw new DirectoryNotFoundException(
                    $"API project directory not found in solution: {API_FOLDER_NAME}"
                );

            return apiDirectory;
        }
        catch (Exception ex)
            when (ex is not DirectoryNotFoundException && ex is not ArgumentNullException)
        {
            throw new InvalidOperationException(
                "An error occurred while resolving the API directory",
                ex
            );
        }
    }

    private static string FindSolutionRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);

        while (directory != null)
        {
            if (directory.GetFiles($"*{SLN_EXTENSION}").Any())
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not find solution root (no {SLN_EXTENSION} file found) starting from: {startPath}"
        );
    }
}