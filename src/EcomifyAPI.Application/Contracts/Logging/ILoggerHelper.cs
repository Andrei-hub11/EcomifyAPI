namespace EcomifyAPI.Application.Contracts.Logging;

public interface ILoggerHelper<T>
{
    void LogWarning(string message);
    void LogInformation(string message);
    void LogError(string message);
    void LogError(Exception exception, string message);
}