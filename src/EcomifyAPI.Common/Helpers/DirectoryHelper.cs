namespace EcomifyAPI.Common.Helpers;

public static class DirectoryHelper
{
    /// <summary>
    /// Finds the specified directory while searching upwards from a base directory.
    /// </summary>
    /// <param name="baseDirectory">The base directory to start searching from.</param>
    /// <param name="targetDirectoryName">The name of the target directory to find.</param>
    /// <returns>The full path to the target directory if found.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the target directory is not found.</exception>
    public static string FindDirectoryAbove(string baseDirectory, string targetFolderName)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            throw new ArgumentException("Base directory cannot be null or whitespace.", nameof(baseDirectory));
        }

        if (string.IsNullOrWhiteSpace(targetFolderName))
        {
            throw new ArgumentException("Target folder name cannot be null or whitespace.", nameof(targetFolderName));
        }

        var currentDirectory = baseDirectory;

        while (!string.IsNullOrWhiteSpace(currentDirectory))
        {
            var subdirectories = Directory.GetDirectories(currentDirectory);

            foreach (var subdirectory in subdirectories)
            {
                if (Path.GetFileName(subdirectory).Equals(targetFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    return subdirectory;
                }
            }

            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
        }

        throw new DirectoryNotFoundException($"The directory '{targetFolderName}' could not be found starting from '{baseDirectory}'.");
    }
}