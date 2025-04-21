
namespace RideTracker.Groups.ListPage;

public class GroupListItem
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public bool IsCurrent { get; set; }

    public Color? BackgroundColor => IsCurrent ? Colors.LightGreen : Colors.LightGrey;
}
