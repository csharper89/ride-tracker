namespace RideTracker.Database;

public class MigrationFailedException : Exception
{
    public MigrationFailedException(string message) : base(message)
    {
    }
}
