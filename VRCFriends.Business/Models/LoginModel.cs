using System.Security;
using System.Threading.Tasks;
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

        public async Task<bool> LoginUserAsync(string username, SecureString password)
        {
            var configuration = (Configuration)GlobalConfiguration.Instance;
            configuration.Username = username ?? string.Empty;
            configuration.Password = password.ToPlainString();

            _stateMediator.OnConfigurationChanged(configuration);

            // Our first request we get the ApiResponse instead of just the user object,
            // so we can see what the API expects from us
            ApiResponse<CurrentUser> currentUserResponse = await _authenticationApi.GetCurrentUserWithHttpInfoAsync().ConfigureAwait(false);            

            // Function that determines if the api expects email2FA from an ApiResponse
            // We can just use a super simple string.Contains() check
            RequiresEmailOtp = currentUserResponse.RawContent.Contains("emailOtp");

            return currentUserResponse.Data != null;
        }

        public bool LoginUser(string username, SecureString password) => LoginUserAsync(username, password).Result;

        public async Task<bool> ValidateOtpAsync(string otpCode, bool requiresEmailOtp)
        {
            bool userVerified;

            if (requiresEmailOtp)
            {
                // If the API wants us to send an Email OTP code
                var response = await _authenticationApi.Verify2FAEmailCodeAsync(new TwoFactorEmailCode(otpCode));
                userVerified = response.Verified;
            }
            else
            {
                // requiresEmail2FA returned false, so we use secret-based 2fa verification
                // authApi.VerifyRecoveryCode(new TwoFactorAuthCode("12345678")); // To Use a Recovery Code
                var response = await _authenticationApi.Verify2FAAsync(new TwoFactorAuthCode(otpCode));
                userVerified = response.Verified;
            }

            if (userVerified)
                _stateMediator.OnUserOtpVerified();

            return userVerified;
        }

        public bool ValidateOtp(string otpCode, bool requiresEmailOtp) => ValidateOtpAsync(otpCode, requiresEmailOtp).Result;

        public CurrentUser GetCurrentUser() => _authenticationApi.GetCurrentUser();

        public async Task<CurrentUser> GetCurrentUserAsync() => await _authenticationApi.GetCurrentUserAsync();
    }
}
