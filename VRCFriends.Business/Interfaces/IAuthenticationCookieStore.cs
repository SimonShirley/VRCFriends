using VRChat.API.Client;

namespace VRCFriends.Business.Interfaces
{
    public interface IAuthenticationCookieStore
    {
        bool LoadAuthenticationCookies(IReadableConfiguration configuration);
        void SaveAuthenticationCookies();
    }
}
