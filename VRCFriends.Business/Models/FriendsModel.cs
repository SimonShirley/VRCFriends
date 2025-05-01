using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Friends;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;

namespace VRCFriends.Business.Models
{
    public class FriendsModel : IFriendsModel, IDisposable
    {
        private readonly IFriendsApi _friendsApi;
        private readonly IStateMediator _stateMediator;
        private readonly ILimitedUserDtoFactory _limitedUserDtoFactory;

        private IList<LimitedUserDto> _onlineFriends = new List<LimitedUserDto>();
        private IList<LimitedUserDto> _offlineFriends = new List<LimitedUserDto>();

        public FriendsModel(
            IFriendsApi friendsApi,
            IStateMediator stateMediator,
            ILimitedUserDtoFactory limitedUserDtoFactory)
        {
            _friendsApi = friendsApi;
            _stateMediator = stateMediator;

            _stateMediator.ConfigurationChanged += StateMediator_ConfigurationChanged;
            _limitedUserDtoFactory = limitedUserDtoFactory;
        }

        private void StateMediator_ConfigurationChanged() => _friendsApi.Configuration = GlobalConfiguration.Instance;

        public DateTime LastRefresh { get; private set; }

        public async Task RefreshFriendsListAsync()
        {
            _offlineFriends.Clear();
            _offlineFriends = await GetFriendsListAsync(false);

            _onlineFriends.Clear();
            _onlineFriends = await GetFriendsListAsync(true);

            LastRefresh = DateTime.Now;

            _stateMediator.OnFriendsListUpdated(_offlineFriends.Union(_onlineFriends).ToList());
        }

        private async Task<IList<LimitedUserDto>> GetFriendsListAsync(bool offline)
        {
            IList<LimitedUserDto> friendDtoList = new List<LimitedUserDto>();

            // update configuration because it may have changed
            _friendsApi.Configuration = GlobalConfiguration.Instance;
            
            IList<LimitedUser> friendsList = _friendsApi.GetFriends(offline: offline) ?? throw new Exception("Unable to connect to VRChat.");
            
            foreach (LimitedUser friend in friendsList)
                friendDtoList.Add(await ConvertApiLimitedUserToDtoUserAsync(friend));

            return friendDtoList;
        }

        public int GetOnlineFriendsCount()
        {
            return _stateMediator.FriendsDictionary.Count(friend => friend.Value.OnlineStatus == OnlineStatusEnum.IsOnline);
        }

        private async Task<LimitedUserDto> ConvertApiLimitedUserToDtoUserAsync(LimitedUser user) => await _limitedUserDtoFactory.ConvertToDtoAsync(user);

        public void Dispose()
        {
            _stateMediator.ConfigurationChanged -= StateMediator_ConfigurationChanged;

            if (_offlineFriends?.Any() ?? false)
                foreach (var friend in _offlineFriends)
                    friend?.Dispose();

            if (_onlineFriends?.Any() ?? false)
                foreach (var friend in _onlineFriends)
                    friend?.Dispose();
        }

        public void RefreshFriendsList() => Task.Run(RefreshFriendsListAsync);
    }
}
