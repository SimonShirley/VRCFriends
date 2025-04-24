using VRCFriends.Business.Interfaces.Friends;
using VRCFriends.Business.Models;
using VRChat.API.Model;

namespace VRCFriends.Business.Factories
{
    public class InstanceDtoFactory : IInstanceDtoFactory
    {
        public InstanceDto Create(string worldName, InstanceType? instanceType = null, InstanceRegion? region = null)
        {
            return new InstanceDto() { WorldName = worldName, InstanceType = instanceType, Region = region };
        }
    }
}
