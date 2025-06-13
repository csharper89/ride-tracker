
namespace RideTracker
{
    public partial class AppShell : Shell
    {
        private readonly GroupUtils _groupUtils;

        public AppShell(GroupUtils groupUtils)
        {
            InitializeComponent();
            _groupUtils = groupUtils;
        }

        protected override async void OnAppearing()
        {
            statsPage.IsVisible = await _groupUtils.IsUserManagingCurrentGroupAsync();
            base.OnAppearing();
        }
    }
}
