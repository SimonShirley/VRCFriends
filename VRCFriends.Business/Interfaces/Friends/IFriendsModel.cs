
using System;

namespace VRCFriends.Business.Interfaces.Friends
{
    public interface IFriendsModel
    {
        DateTime LastRefresh { get; }

        int GetOnlineFriendsCount();
        void RefreshFriendsList();
    }
}
