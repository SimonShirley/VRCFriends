using CommunityToolkit.Mvvm.ComponentModel;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Friends;
using VRCFriends.Business.Models;
using Timer = System.Threading.Timer;

namespace VRCFriends.ViewModels
{
    public partial class FriendsListViewModel : ViewModelBase, IFriendsListViewModel, IDisposable
    {
        private readonly IFriendsModel? _friendsModel;
        private readonly IStateMediator _stateMediator;
        private Timer? _timer;

        [ObservableProperty]
        private bool _showRefreshProgressBar;

        public string LastRefresh => (_friendsModel?.LastRefresh.ToLocalTime() ?? DateTime.MinValue).ToString("MMMM dd, yyyy HH:mm");

        public IEnumerable<LimitedUserDto> OnlineFriendsCollection => [.. GetFriendsListQuery(OnlineStatusEnum.IsOnline)];

        public IEnumerable<LimitedUserDto> OnlineAnotherPlatformFriendsCollection => [.. GetFriendsListQuery(OnlineStatusEnum.IsOnAnotherPlatform)];

        public IEnumerable<LimitedUserDto> OfflineFriendsCollection => [.. GetFriendsListQuery(OnlineStatusEnum.IsOffline)];

        public FriendsListViewModel(IFriendsModel friendsModel, IStateMediator stateMediator)
        {
            _stateMediator = stateMediator;
            _stateMediator.FriendsListUpdated += StateMediator_FriendsListUpdated;
            _stateMediator.UserOtpVerified += StateMediator_UserOtpVerified;

            _friendsModel = friendsModel;
        }

        private void StateMediator_UserOtpVerified()
        {
            if (_friendsModel is not null && _timer is null)
                _timer = new Timer(new TimerCallback(Timer_TimerCallback), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        private async void Timer_TimerCallback(object? state)
        {
            if (_friendsModel is not null)
            {
                ShowRefreshProgressBar = true;

                await _friendsModel.RefreshFriendsListAsync();

                ShowRefreshProgressBar = false;
            }
        }

        private void StateMediator_FriendsListUpdated()
        {
            OnPropertyChanged(nameof(OnlineFriendsCollection));
            OnPropertyChanged(nameof(OnlineAnotherPlatformFriendsCollection));
            OnPropertyChanged(nameof(OfflineFriendsCollection));
            OnPropertyChanged(nameof(LastRefresh));
        }

        private IOrderedEnumerable<LimitedUserDto> GetFriendsListQuery(OnlineStatusEnum onlineStatus)
        {
            return _stateMediator.FriendsDictionary
                .Where(friend => friend.Value.OnlineStatus == onlineStatus)
                .Select(friend => friend.Value)
                .OrderBy(friend => friend.DisplayName);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _timer?.Dispose();

            _stateMediator.FriendsListUpdated -= StateMediator_FriendsListUpdated;
            _stateMediator.UserOtpVerified -= StateMediator_UserOtpVerified;
        }
    }
}
