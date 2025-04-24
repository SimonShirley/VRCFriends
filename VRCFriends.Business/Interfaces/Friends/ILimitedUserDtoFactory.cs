using System.Threading.Tasks;
using VRCFriends.Business.Models;
using VRChat.API.Model;

namespace VRCFriends.Business.Interfaces.Friends
{
    public interface ILimitedUserDtoFactory
    {
        LimitedUserDto ConvertToDto(LimitedUser user);
        Task<LimitedUserDto> ConvertToDtoAsync(LimitedUser user);
    }
}
