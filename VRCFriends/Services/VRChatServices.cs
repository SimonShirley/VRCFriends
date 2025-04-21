using Microsoft.Extensions.DependencyInjection;
using VRChat.API.Api;
using VRChat.API.Client;

namespace VRCFriends.Services
{
    public static class VRChatServices
    {
        public static void AddVRChatApiServices(this IServiceCollection services)
        {
            services.AddTransient<IAuthenticationApi, AuthenticationApi>(provider => new AuthenticationApi((Configuration)GlobalConfiguration.Instance));
            services.AddTransient<IFriendsApi, FriendsApi>(provider => new FriendsApi((Configuration)GlobalConfiguration.Instance));
            services.AddTransient<IWorldsApi, WorldsApi>(provider => new WorldsApi((Configuration)GlobalConfiguration.Instance));
            services.AddTransient<IInstancesApi, InstancesApi>(provider => new InstancesApi((Configuration)GlobalConfiguration.Instance));
        }
    }
}
