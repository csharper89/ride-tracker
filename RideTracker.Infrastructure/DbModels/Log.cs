using RideTracker.Infrastructure.DbModels.Interfaces;
using SQLite;

namespace RideTracker.Infrastructure.DbModels;

[Table("Logs")]
public class Log
{
    [PrimaryKey]
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Message { get; set; } = null!;
    public string Class { get; set; } = null!;
    public int Level { get; set; }
}
