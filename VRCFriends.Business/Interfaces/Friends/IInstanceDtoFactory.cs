using VRCFriends.Business.Models;
using VRChat.API.Model;

namespace VRCFriends.Business.Interfaces.Friends
{
    public interface IInstanceDtoFactory
    {
        InstanceDto Create(string worldName, InstanceType? instanceType = null, InstanceRegion? region = null);
    }
}
