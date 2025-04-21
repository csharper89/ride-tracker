
using SQLite;
using TableAttribute = SQLite.TableAttribute;

namespace RideTracker.Infrastructure.DbModels;

[Table("Settings")]
public class Setting
{
    [PrimaryKey]
    public string Key { get; init; } = null!;
    public string Value { get; set; } = null!;
}
