using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var offlineFriends = await GetFriendsListAsync(false);
            var onlineFriends = await GetFriendsListAsync(true);

            LastRefresh = DateTime.Now;

            _stateMediator.OnFriendsListUpdated(offlineFriends.Union(onlineFriends).ToList());
        }

        private async Task<IList<LimitedUserDto>> GetFriendsListAsync(bool offline)
        {
            IList<LimitedUserDto> friendDtoList = new List<LimitedUserDto>();

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
        }

        public void RefreshFriendsList() => Task.Run(() => RefreshFriendsListAsync());
    }
}
