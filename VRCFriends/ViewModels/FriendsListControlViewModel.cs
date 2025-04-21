using CommunityToolkit.Mvvm.ComponentModel;
using VRChat.API.Model;

namespace VRCFriends.ViewModels
{
    public partial class FriendsListControlViewModel : ViewModelBase
    {

        [ObservableProperty]
        private string? _groupHeader;

        [ObservableProperty]
        private IEnumerable<LimitedUser>? _friendsCollection;

    }
}
