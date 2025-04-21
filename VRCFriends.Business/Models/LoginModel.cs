using System.Security;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Login;
using VRCFriends.Extensions;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;

namespace VRCFriends.Business.Models
{
    public class LoginModel : ILoginModel
    {
        private readonly IStateMediator _stateMediator;  
        private readonly IAuthenticationApi _authenticationApi;

        public bool RequiresEmailOtp { get; set; }

        public LoginModel(IStateMediator stateMediator, IAuthenticationApi authenticationApi)
        {
            _stateMediator = stateMediator;
            _authenticationApi = authenticationApi;
        }

        public bool LoginUser(string username, SecureString password, out bool requiresEmailOtp)
        {
            var configuration = (Configuration)GlobalConfiguration.Instance;
            configuration.Username = username ?? string.Empty;
            configuration.Password = password.ToPlainString();

            _stateMediator.OnConfigurationChanged(configuration);

            // Our first request we get the ApiResponse instead of just the user object,
            // so we can see what the API expects from us
            ApiResponse<CurrentUser> currentUserResponse = _authenticationApi.GetCurrentUserWithHttpInfo();            

            // Function that determines if the api expects email2FA from an ApiResponse
            // We can just use a super simple string.Contains() check
            requiresEmailOtp = currentUserResponse.RawContent.Contains("emailOtp");

            return currentUserResponse.Data != null;
        }

        public bool ValidateOtp(string otpCode, bool requiresEmailOtp)
        {
            bool userVerified;

            if (requiresEmailOtp)
            {
                // If the API wants us to send an Email OTP code
                userVerified = _authenticationApi.Verify2FAEmailCode(new TwoFactorEmailCode(otpCode)).Verified;                
            }
            else
            {
                // requiresEmail2FA returned false, so we use secret-based 2fa verification
                // authApi.VerifyRecoveryCode(new TwoFactorAuthCode("12345678")); // To Use a Recovery Code
                userVerified = _authenticationApi.Verify2FA(new TwoFactorAuthCode(otpCode)).Verified;
            }

            if (userVerified)
                _stateMediator.OnUserOtpVerified();

            return userVerified;
        }

        public CurrentUser GetCurrentUser() => _authenticationApi.GetCurrentUser();
    }
}
