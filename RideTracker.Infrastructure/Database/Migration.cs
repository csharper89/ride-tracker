namespace RideTracker.Database;

public class Migration
{
    public int Version { get; private set; }
    public string MigrationFileName { get; private set; }

    public Migration(string lineFromMigrationList)
    {
        var parts = lineFromMigrationList.Split(' ');
        if(parts.Length != 2)
        {
            throw new MigrationFailedException("Migration list is not in the correct format");
        }

        if(!int.TryParse(parts[0], out var version))
        {
            throw new MigrationFailedException("Migration version is not a number");
        }

        Version = version;
        MigrationFileName = parts[1];
    }
}
