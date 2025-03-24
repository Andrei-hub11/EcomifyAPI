using EcomifyAPI.Application.Contracts.Logging;

using Microsoft.Extensions.Logging;

namespace EcomifyAPI.Infrastructure.Logging;

public class LoggerHelper<T> : ILoggerHelper<T>
{
    private readonly ILogger<T> _logger;

    public LoggerHelper(ILogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogWarning(string message)
    {
        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning("{Message}", message);
        }
    }

    public void LogInformation(string message)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{Message}", message);
        }
    }

    public void LogError(string message)
    {
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("{Message}", message);
        }
    }

    public void LogError(Exception exception, string message)
    {
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError(exception, "{Message}", message);
        }
    }
}