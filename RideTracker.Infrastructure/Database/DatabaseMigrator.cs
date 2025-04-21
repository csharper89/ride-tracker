using Microsoft.Maui.Storage;
using RideTracker.Database;
using RideTracker.Infrastructure.Constants;
using RideTracker.Infrastructure.DbModels;
using SQLite;
using System.Text.RegularExpressions;

namespace RideTracker.Infrastructure.Database;

public class DatabaseMigrator(SQLiteAsyncConnection db, TimeProvider timeProvider)
{
    public async Task MigrateAsync()
    {
        await CreateBasicTablesIfTheyDoNotExistAsync();
        var allMigrations = GetAllMigrationListAsync();
        var appliedMigrationVersions = await GetAppliedMigrationVersionsAsync();
        var migrationsToApply = GetMigrationsToApply(allMigrations, appliedMigrationVersions);
        foreach (var migration in migrationsToApply)
        {
            await RunMigrationScriptAsync(migration.MigrationFileName);
            await AddHistoryRecordAsync(migration);
        }
    }

    private List<Migration> GetAllMigrationListAsync()
    {
        using var migrationListStream = FileSystem.OpenAppPackageFileAsync(Paths.MigrationListPath).GetAwaiter().GetResult();
        using var sr = new StreamReader(migrationListStream);
        var lineFromMigrationList = sr.ReadLineAsync().GetAwaiter().GetResult();
        var list = new List<Migration>();
        while (!string.IsNullOrEmpty(lineFromMigrationList))
        {
            var migration = new Migration(lineFromMigrationList);
            list.Add(migration);
            lineFromMigrationList = sr.ReadLineAsync().GetAwaiter().GetResult();
        }

        return list;
    }

    private async Task<HashSet<int>> GetAppliedMigrationVersionsAsync()
    {
        var appliedMigrations = await db.Table<MigrationHistory>().ToListAsync();
        return appliedMigrations
            .Select(x => x.Version)
            .ToHashSet();
    }

    private List<Migration> GetMigrationsToApply(List<Migration> allMigrations, HashSet<int> appliedMigrations)
    {
        return allMigrations
            .Where(x => !appliedMigrations.Contains(x.Version))
            .ToList();
    }

    private async Task RunMigrationScriptAsync(string migrationFileName)
    {
        var migrationResourceFilePath = Path.Combine("Migrations", migrationFileName);
        await using var migrationFileStream = await FileSystem.OpenAppPackageFileAsync(migrationResourceFilePath);
        using var sr = new StreamReader(migrationFileStream);
        var sql = await sr.ReadToEndAsync();
        var sqlStatements = SplitSqlStatements(sql);
        foreach (var sqlStatement in sqlStatements)
        {
            await db.ExecuteAsync(sqlStatement);
        }
    }

    private async Task AddHistoryRecordAsync(Migration migration)
    {
        var historyRecord = new MigrationHistory
        {
            Version = migration.Version,
            MigrationFileName = migration.MigrationFileName,
            AppliedAt = timeProvider.GetUtcNow().DateTime
        };
        await db.InsertAsync(historyRecord);
    }

    private async Task CreateBasicTablesIfTheyDoNotExistAsync()
    {
        await db.CreateTableAsync<Log>();
        await db.CreateTableAsync<MigrationHistory>();
    }

    private string[] SplitSqlStatements(string sql)
    {
        return Regex.Split(sql, "^-+\r?$", RegexOptions.Multiline)
            .Select(x => x.Trim())
            .ToArray();
    }
}