﻿using System.Diagnostics;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Friends;
using VRCFriends.Business.Interfaces.Login;
using VRChat.API.Api;
using VRChat.API.Client;

namespace VRCFriends.Services
{
    public class ViewModelNavigationService : IViewModelNavigationService, IDisposable
    {
        private readonly IStateMediator _stateMediator;
        private readonly IViewModelGeneratorFactory _viewModelFactory;
        private readonly IAuthenticationApi _authenticationApi;

        public ViewModelNavigationService(IStateMediator stateMediator, IViewModelGeneratorFactory abstractFactory, IAuthenticationApi authenticationApi)
        {
            _stateMediator = stateMediator;
            _viewModelFactory = abstractFactory;
            _authenticationApi = authenticationApi;

            _stateMediator.AppStarted += StateMediator_AppStarted;
            _stateMediator.UserRequiresLogin += StateMediator_LoginRequired;
            _stateMediator.UsernamePasswordAccepted += StateMediator_UsernamePasswordAccepted;
            _stateMediator.UserOtpVerified += StateMediator_UserOtpVerified;
        }

        private async void StateMediator_AppStarted()
        {
            // create MainWindowViewModel
            // set StateMediator to current viewmodel
            // call MainWindowViewModel.ShowWindow() to show app window

            try
            {
                var authTokenValid = await _authenticationApi.VerifyAuthTokenAsync();

                // if we can get the current user without an error,
                // we must be validated
                if (authTokenValid is not null && authTokenValid.Ok)
                {
                    _stateMediator.OnUserOtpVerified();
                    return;
                }
            }
            catch (ApiException apiEx)
            {
#if DEBUG
                Debug.WriteLine(apiEx.Message);
                Debug.WriteLine(apiEx.ErrorCode);
                Debug.WriteLine(apiEx.ErrorContent);
#endif
            }

            _stateMediator.OnUserRequiresLogin();
        }

        private void StateMediator_LoginRequired()
        {
            GetViewModel<ILoginUsernamePasswordViewModel>();
        }

        private void StateMediator_UsernamePasswordAccepted(bool requiresTwoFactorAuth, bool requiresEmailOtp)
        {
            if (requiresTwoFactorAuth)
                GetViewModel<ILoginOtpViewModel>();
        }

        private void StateMediator_UserOtpVerified()
        {
            GetViewModel<IFriendsListViewModel>();
        }

        private void GetViewModel<TViewModel>()
        {
            if (_stateMediator.CurrentViewModel is IDisposable disposable)
                disposable.Dispose();

            _stateMediator.OnCurrentViewModelChanged(_viewModelFactory.GetViewModel<TViewModel>()!);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _stateMediator.AppStarted -= StateMediator_AppStarted;
            _stateMediator.UserRequiresLogin -= StateMediator_LoginRequired;
            _stateMediator.UsernamePasswordAccepted -= StateMediator_UsernamePasswordAccepted;
            _stateMediator.UserOtpVerified -= StateMediator_UserOtpVerified;
        }
    }
}
