using RideTracker.Infrastructure.Constants;
using RideTracker.Infrastructure.DbModels;
using SQLite;

namespace RideTracker.Infrastructure.Database;

public class DatabaseInitializer(SQLiteAsyncConnection db)
{
    public async Task CreateIfNotExistsAsync()
    {
        if (!File.Exists(Paths.PathToDatabase))
        {
            await CreateDatabaseAsync();
        }
    }

    private async Task CreateDatabaseAsync()
    {
        await db.CreateTableAsync<MigrationHistory>();
    }
}