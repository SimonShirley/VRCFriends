using System.Security;
using VRChat.API.Model;

namespace VRCFriends.Business.Interfaces.Login
{
    public interface ILoginModel
    {
        bool RequiresEmailOtp { get; set; }

        CurrentUser GetCurrentUser();
        bool LoginUser(string username, SecureString password, out bool requiresEmailTotp);
        bool ValidateOtp(string totpCode, bool requiresEmailTotp);
    }
}
