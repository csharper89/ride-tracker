namespace RideTracker.Groups.Details;

public partial class GroupDetailsPage : ContentPage
{
    private readonly GroupDetailsViewModel _viewModel;

    public string? GroupId { get; set; }

    public GroupDetailsPage(GroupDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _viewModel.InitializeAsync();
    }
}