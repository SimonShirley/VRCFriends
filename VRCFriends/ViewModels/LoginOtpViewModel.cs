using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Login;
using VRChat.API.Client;
using VRChat.API.Model;

namespace VRCFriends.ViewModels
{
    public partial class LoginOtpViewModel : ViewModelBase, ILoginOtpViewModel
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ValidateOtpCommand))]
        private string? _otpCode;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        private readonly IStateMediator _stateMediator;

        public bool ErrorMessageVisible => !string.IsNullOrWhiteSpace(ErrorMessage);

        private static bool ValidateOtpCanExecute(string otpCode) => !string.IsNullOrWhiteSpace(otpCode ?? string.Empty);

        private readonly ILoginModel _loginModel;

        public LoginOtpViewModel(ILoginModel loginModel, IStateMediator stateMediator)
        {
            _loginModel = loginModel;
            _stateMediator = stateMediator;
        }

        [RelayCommand(CanExecute = nameof(ValidateOtpCanExecute))]
        public async Task ValidateOtpAsync(string otpCode)
        {
            try
            {
                // await needs to run on captured context [ ConfigureAwait(true) ] because of file locking
                bool loginVerified = await _loginModel.ValidateOtpAsync(otpCode, _loginModel.RequiresEmailOtp).ConfigureAwait(true);

                if (loginVerified)
                {
                    OtpCode = string.Empty;

                    CurrentUser currentUser = await _loginModel.GetCurrentUserAsync();
                    Debug.WriteLine("Logged in as {0}", currentUser.DisplayName);

                    _stateMediator.OnUserOtpVerified();
                }
                else
                {
                    ErrorMessage = "Login verification failed";
                    Debug.WriteLine(ErrorMessage);
                }
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.Message;

                if (apiEx.ErrorCode == 401)
                    ErrorMessage = "Incorrect One Time Passcode";

                // Catch any exceptions write to console, helps w debugging :D
                Debug.WriteLine("Exception when calling API: {0}", apiEx.Message);
                Debug.WriteLine("Status Code: {0}", apiEx.ErrorCode);
                Debug.WriteLine(apiEx.ToString());
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;

                // Catch any exceptions write to console, helps w debugging :D
                Debug.WriteLine("Exception when calling API: {0}", ex.Message);
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
