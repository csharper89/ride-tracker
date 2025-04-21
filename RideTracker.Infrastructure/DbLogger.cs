using Microsoft.Extensions.Logging;
using RideTracker.Infrastructure.DbModels;
using SQLite;

namespace RideTracker.Infrastructure;

public class DbLogger<T>(ISQLiteAsyncConnection db, TimeProvider timeProvider)
{
    public async Task LogDebug(string message)
    {
        await Log(message, LogLevel.Debug);
    }

    public async Task LogInformation(string message)
    {
        await Log(message, LogLevel.Information);
    }

    public async Task LogWarning(string message)
    {
        await Log(message, LogLevel.Warning);
    }

    public async Task LogError(string message)
    {
        await Log(message, LogLevel.Error);
    }

    public async Task LogError(Exception ex, string message)
    {
        var fullMessage = $"{message} - Exception: {ex.Message}\n{ex.StackTrace}";
        await Log(fullMessage, LogLevel.Error);
    }

    private async Task Log(string message, LogLevel level)
    {
        var log = new Log
        {
            Id = Guid.NewGuid(),
            CreatedAt = timeProvider.GetUtcNow().DateTime,
            Message = message,
            Class = typeof(T).Name,
            Level = (int)level
        };

        await db.InsertAsync(log);
    }
}