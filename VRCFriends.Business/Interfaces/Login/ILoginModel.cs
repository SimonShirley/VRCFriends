using System.Security;
using System.Threading.Tasks;
using VRChat.API.Model;

namespace VRCFriends.Business.Interfaces.Login
{
    public interface ILoginModel
    {
        bool RequiresTwoFactorAuth { get; }
        bool RequiresEmailOtp { get; }

        CurrentUser GetCurrentUser();
        Task<CurrentUser> GetCurrentUserAsync();
        bool LoginUser(string username, SecureString password);
        Task<bool> LoginUserAsync(string username, SecureString password);
        bool ValidateOtp(string totpCode, bool requiresEmailTotp);
        ValueTask<bool> ValidateOtpAsync(string otpCode, bool requiresEmailOtp);
    }
}
