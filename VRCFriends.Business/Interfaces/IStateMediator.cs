using System;
using System.Collections.Generic;
using VRCFriends.Business.Models;
using VRChat.API.Client;

namespace VRCFriends.Business.Interfaces
{
    public interface IStateMediator
    {
        IViewModel CurrentViewModel { get; }
        IDictionary<string, LimitedUserDto> FriendsDictionary { get; }
        string AppDataPath { get; }

        event Action ConfigurationChanged;
        event Action CurrentViewModelChanged;
        event Action<bool, bool> UsernamePasswordAccepted;
        event Action UserOtpVerified;
        event Action AppStarted;
        event Action UserRequiresLogin;
        event Action FriendsListUpdated;
        event Action AppExited;
        event Action ShowFriendsContextMenuItemClicked;

        void OnAppExited();
        void OnAppStarted();
        void OnConfigurationChanged(IReadableConfiguration configuration);
        void OnCurrentViewModelChanged(IViewModel currentViewModel);
        void OnFriendsListUpdated(IList<LimitedUserDto> friendsList);
        void OnShowFriendsContextMenuItemClicked();
        void OnUsernamePasswordAccepted(bool requiresTwoFactorAuth, bool requiresEmailOtp);
        void OnUserOtpVerified();
        void OnUserRequiresLogin();
    }
}
