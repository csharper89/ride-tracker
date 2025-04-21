using SQLite;

namespace RideTracker.Infrastructure.DbModels;

public class MigrationHistory
{
    [PrimaryKey]
    public int Version { get; init; }
    public string MigrationFileName { get; init; } = null!;
    public DateTime AppliedAt { get; init; }
}