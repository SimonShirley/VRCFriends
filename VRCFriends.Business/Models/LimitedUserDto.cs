using System;
using System.Drawing;
using VRChat.API.Model;

namespace VRCFriends.Business.Models
{
    public class LimitedUserDto : IDisposable
    {
        public string Id { get; set; }
        public Image CurrentAvatarImage { get; set; }
        public string DisplayName { get; set; }
        public InstanceDto Location { get; set; }
        public UserStatus Status { get; set; }
        public OnlineStatusEnum OnlineStatus { get; set; }

        public void Dispose()
        {
            CurrentAvatarImage?.Dispose();
        }
    }
}
