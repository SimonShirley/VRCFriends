﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Security;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Login;
using VRChat.API.Client;

namespace VRCFriends.ViewModels
{
    public partial class LoginUsernamePasswordViewModel : ViewModelBase, ILoginUsernamePasswordViewModel
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginUserCommand))]
        private string? _vrcUsername;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginUserCommand))]
        private SecureString _vrcPassword;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ErrorMessageVisible))]
        private string _errorMessage = string.Empty;

        public bool ErrorMessageVisible => !string.IsNullOrWhiteSpace(ErrorMessage);

        private bool UsernamePasswordLoginCanExecute => !string.IsNullOrWhiteSpace(VrcUsername) && VrcPassword.Length > 0;

        private readonly ILoginModel _loginModel;

        private readonly IStateMediator _stateMediator;

        public LoginUsernamePasswordViewModel(ILoginModel loginModel, IStateMediator stateMediator)
        {
            _vrcPassword = new SecureString();
            _loginModel = loginModel;
            _stateMediator = stateMediator;
        }

        [RelayCommand(CanExecute = nameof(UsernamePasswordLoginCanExecute))]
        public async Task LoginUserAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                if (await _loginModel.LoginUserAsync(VrcUsername ?? string.Empty, VrcPassword))
                {
                    _stateMediator.OnUsernamePasswordAccepted(_loginModel.RequiresTwoFactorAuth, _loginModel.RequiresEmailOtp);

                    if (!_loginModel.RequiresTwoFactorAuth)
                        _stateMediator.OnUserOtpVerified();
                }
                else
                    ErrorMessage = "Unable to login.";
            }
            catch (ApiException apiEx)
            {
                ErrorMessage = apiEx.Message;

                if (apiEx.ErrorCode == 401)
                    ErrorMessage = "Invalid username or password.";

#if DEBUG
                // Catch any exceptions write to console, helps w debugging :D
                Debug.WriteLine("Exception when calling API: {0}", apiEx.Message);
                Debug.WriteLine("Status Code: {0}", apiEx.ErrorCode);
                Debug.WriteLine(apiEx.ToString());
#endif
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;

#if DEBUG
                // Catch any exceptions write to console, helps w debugging :D
                Debug.WriteLine("Exception when calling API: {0}", ex.Message);
                Debug.WriteLine(ex.ToString());
#endif
            }
        }

        void ILoginUsernamePasswordViewModel.LoginUser() => Task.Run(LoginUserAsync);
    }
}
