using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Friends;
using VRCFriends.Business.Models;
using VRChat.API.Client;
using Timer = System.Threading.Timer;

namespace VRCFriends.ViewModels
{
    public partial class FriendsListViewModel : ViewModelBase, IFriendsListViewModel, IDisposable
    {
        private readonly IFriendsModel? _friendsModel;
        private readonly IStateMediator _stateMediator;
        private Timer? _automaticRefreshTimer;
        private Timer? _manualRefreshTimer;
        private bool _canRefreshFriendsListManually = false;

        public bool CanManuallyRefreshFriendsList {
            get => _canRefreshFriendsListManually;
            private set
            {
                _canRefreshFriendsListManually = value;
                OnPropertyChanged(nameof(CanManuallyRefreshFriendsList));
                App.Current.Dispatcher.BeginInvoke(ManuallyRefreshFriendsListCommand.NotifyCanExecuteChanged);
            }
        }

        [ObservableProperty]
        private bool _showRefreshProgressBar;

        [ObservableProperty]
        private string _statusMessage = "";

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
            if (_friendsModel is not null && _automaticRefreshTimer is null)
            {
                _automaticRefreshTimer?.Dispose();
                _automaticRefreshTimer = new Timer(new TimerCallback(async (obj) => await RefreshFriendsList(obj)), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

                _manualRefreshTimer?.Dispose();
                _manualRefreshTimer = new Timer(new TimerCallback((obj) => CanManuallyRefreshFriendsList = true), null, TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
            }
        }

        private async ValueTask RefreshFriendsList(object? state)
        {
            try
            {
                StatusMessage = string.Empty;
                CanManuallyRefreshFriendsList = false;

                if (_friendsModel is not null)
                {
                    ShowRefreshProgressBar = true;

                    await _friendsModel.RefreshFriendsListAsync();

                    ShowRefreshProgressBar = false;

                    _manualRefreshTimer?.Change(TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
                    _automaticRefreshTimer?.Change(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
                }
                else
                    StatusMessage = "An error occured.";
            }
            catch (ApiException apiEx)
            {
                StatusMessage = apiEx.Message;

#if DEBUG
                Debug.WriteLine(apiEx.Message);
                Debug.WriteLine(apiEx.ErrorCode);
                Debug.WriteLine(apiEx.ErrorContent);
#endif
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;

#if DEBUG
                Debug.WriteLine(ex.Message);
#endif
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

        [RelayCommand(CanExecute = nameof(CanManuallyRefreshFriendsList))]
        private async Task ManuallyRefreshFriendsList() => await RefreshFriendsList(null);

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _manualRefreshTimer?.Dispose();
            _automaticRefreshTimer?.Dispose();

            _stateMediator.FriendsListUpdated -= StateMediator_FriendsListUpdated;
            _stateMediator.UserOtpVerified -= StateMediator_UserOtpVerified;
        }
    }
}
