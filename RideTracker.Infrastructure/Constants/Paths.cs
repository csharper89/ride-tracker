using Microsoft.Maui.Storage;

namespace RideTracker.Infrastructure.Constants;

public static class Paths
{
    public static string PathToDatabase => Path.Combine(FileSystem.AppDataDirectory, "Database.db");
    public const string MigrationListPath = "Migrations/list.txt";
}