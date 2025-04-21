using System.Drawing;
using VRChat.API.Model;

namespace VRCFriends.Business.Models
{
    public class LimitedUserDto
    {
        public string Id { get; set; }
        public Bitmap CurrentAvatarImage { get; set; }
        public string DisplayName { get; set; }
        public InstanceDto Location { get; set; }
        public UserStatus Status { get; set; }
        public OnlineStatusEnum OnlineStatus { get; set; }

    }
}
