namespace RideTracker.Groups.List;

public partial class GroupsListPage : ContentPage
{
    private readonly GroupListModel _model;

    public GroupsListPage(GroupListModel model)
    {
        InitializeComponent();
        _model = model;
        BindingContext = model;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _model.LoadGroupsAsync();
    }
}