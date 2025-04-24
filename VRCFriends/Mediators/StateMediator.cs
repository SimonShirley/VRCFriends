using System.IO;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Models;
using VRChat.API.Client;

namespace VRCFriends.Mediators
{
    public class StateMediator : IStateMediator
    {
        private readonly string? _appDataPath;
        private readonly IAuthenticationCookieStore _cookieStore;

        public IViewModel? CurrentViewModel { get; private set; }

        public IDictionary<string, LimitedUserDto> FriendsDictionary { get; private set; } = new Dictionary<string, LimitedUserDto>();

        public string? AppDataPath { get => _appDataPath; }

        public event Action? ConfigurationChanged;

        public event Action? CurrentViewModelChanged;

        public event Action? UserRequiresLogin;

        public event Action<bool, bool>? UsernamePasswordAccepted;

        public event Action? UserOtpVerified;

        public event Action? AppStarted;

        public event Action? FriendsListUpdated;

        public event Action? AppExited;

        public event Action? ShowFriendsContextMenuItemClicked;

        public StateMediator(IAuthenticationCookieStore cookieStore)
        {
            _cookieStore = cookieStore;
            _appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vrcfriends");
        }

        public void OnConfigurationChanged(IReadableConfiguration configuration)
        {
            GlobalConfiguration.Instance = Configuration.MergeConfigurations(GlobalConfiguration.Instance, configuration);
            ConfigurationChanged?.Invoke();
        }

        public void OnCurrentViewModelChanged(IViewModel? currentViewModel)
        {
            CurrentViewModel = currentViewModel;
            CurrentViewModelChanged?.Invoke();
        }

        public void OnUserRequiresLogin()
        {
            UserRequiresLogin?.Invoke();
        }

        public void OnUsernamePasswordAccepted(bool requiresTwoFactorAuth, bool requiresEmailOtp)
        {
            UsernamePasswordAccepted?.Invoke(requiresTwoFactorAuth, requiresEmailOtp);
        }

        public void OnUserOtpVerified()
        {
            _cookieStore.SaveAuthenticationCookies();

            var configuration = new Configuration
            {
                Username = string.Empty,
                Password = string.Empty,
                UserAgent = GlobalConfiguration.Instance.UserAgent,
                DefaultHeaders = GlobalConfiguration.Instance.DefaultHeaders,
            };

            OnConfigurationChanged(configuration);

            UserOtpVerified?.Invoke();
        }

        public void OnAppStarted()
        {
            OnConfigurationChanged(GlobalConfiguration.Instance);

            AppStarted?.Invoke();
        }

        public void OnAppExited()
        {
            AppExited?.Invoke();
        }

        public void OnFriendsListUpdated(IList<LimitedUserDto> friendsList)
        {
            foreach (var friend in friendsList)
            {
                if (FriendsDictionary.ContainsKey(friend.Id))
                    FriendsDictionary[friend.Id] = friend;
                else
                    FriendsDictionary.Add(friend.Id, friend);
            }

            FriendsListUpdated?.Invoke();
        }

        public void OnShowFriendsContextMenuItemClicked()
        {
            ShowFriendsContextMenuItemClicked?.Invoke();
        }
    }
}
