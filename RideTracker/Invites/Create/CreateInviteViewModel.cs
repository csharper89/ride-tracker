using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Infrastructure;
namespace RideTracker.Invites;

[QueryProperty(nameof(GroupId), nameof(GroupId))]
public partial class CreateInviteViewModel(RideTrackerHttpClient httpClient, DbLogger<CreateInviteViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isCodeVisible;

    [ObservableProperty]
    private Guid _groupId;

    [ObservableProperty]
    private string _inviteCode;

    [RelayCommand]
    private async Task CreateInviteAsync()
    {
        logger.LogInformation("CreateInviteAsync operation started.");

        var request = new CreateInviteRequest
        {
            IsAdmin = IsAdmin,
            GroupId = GroupId
        };

        try
        {
            logger.LogInformation($"Sending invite creation request for GroupId: {GroupId} with IsAdmin: {IsAdmin}");
            var response = await httpClient.PostAsync<CreateInviteResponse>("api/invites", request);
            InviteCode = response.Code;
            IsCodeVisible = true;
            logger.LogInformation($"Invite created successfully with Code: {InviteCode}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during invite creation.");
        }
    }
}