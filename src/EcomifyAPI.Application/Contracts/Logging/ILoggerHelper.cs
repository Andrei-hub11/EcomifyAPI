namespace EcomifyAPI.Application.Contracts.Logging;

public interface ILoggerHelper<T>
{
    void LogWarning(string message);
    void LogInformation(string message, params object[] args);
    void LogError(string message);
    void LogError(Exception exception, string message);
}