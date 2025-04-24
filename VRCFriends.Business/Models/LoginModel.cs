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

        public bool RequiresTwoFactorAuth { get; private set; }
        public bool RequiresEmailOtp { get; private set; }

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

            _authenticationApi.Configuration = Configuration.MergeConfigurations(GlobalConfiguration.Instance, configuration);

            // Our first request we get the ApiResponse instead of just the user object,
            // so we can see what the API expects from us
            ApiResponse<CurrentUser> currentUserResponse = await _authenticationApi.GetCurrentUserWithHttpInfoAsync().ConfigureAwait(false);

            // Function that determines if the api expects TwoFactorAuth from an ApiResponse
            // We can just use a super simple string.Contains() check
            RequiresTwoFactorAuth = currentUserResponse.RawContent.Contains("requiresTwoFactorAuth");

            // Function that determines if the api expects email2FA from an ApiResponse
            // We can just use a super simple string.Contains() check
            RequiresEmailOtp = currentUserResponse.RawContent.Contains("emailOtp");

            return currentUserResponse.Data != null;
        }

        public bool LoginUser(string username, SecureString password) => LoginUserAsync(username, password).Result;

        public async Task<bool> ValidateOtpAsync(string otpCode, bool requiresEmailOtp)
        {
            if (string.IsNullOrWhiteSpace(otpCode))
                return false;

            otpCode = otpCode.Trim();

            if (otpCode.Length != 6 && otpCode.Length != 9)
                return false;

            if (otpCode.Length == 6)
            {
                if (requiresEmailOtp)
                {
                    // If the API wants us to send an Email OTP code
                    var response = await _authenticationApi.Verify2FAEmailCodeAsync(new TwoFactorEmailCode(otpCode));
                    return response.Verified;
                }
                else
                {
                    // requiresEmail2FA returned false, so we use secret-based 2fa verification
                    // authApi.VerifyRecoveryCode(new TwoFactorAuthCode("12345678")); // To Use a Recovery Code
                    var response = await _authenticationApi.Verify2FAAsync(new TwoFactorAuthCode(otpCode));
                    return response.Verified;
                }
            }

            if (otpCode.Length == 9)
            {
                var response = await _authenticationApi.VerifyRecoveryCodeAsync(new TwoFactorAuthCode(otpCode));
                return response.Verified;
            }

            return false;
        }

        public bool ValidateOtp(string otpCode, bool requiresEmailOtp) => ValidateOtpAsync(otpCode, requiresEmailOtp).Result;

        public CurrentUser GetCurrentUser() => _authenticationApi.GetCurrentUser();

        public async Task<CurrentUser> GetCurrentUserAsync() => await _authenticationApi.GetCurrentUserAsync();
    }
}
