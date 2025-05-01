using Microsoft.Extensions.DependencyInjection;
using VRCFriends.Factories;
using VRCFriends.Business.Interfaces;
using VRCFriends.Business.Interfaces.Friends;
using VRCFriends.Business.Interfaces.Login;
using VRCFriends.Mediators;
using VRCFriends.Business.Models;
using VRCFriends.NotifyIcon;
using VRCFriends.ViewModels;
using VRCFriends.Views;
using VRCFriends.Business.Factories;

namespace VRCFriends.Services
{
    public static class VRCFriendsServices
    {
        public static void AddVRCFriendsServices(this IServiceCollection services)
        {
            services.AddSingleton<IMainWindowView, MainWindowView>();

            services.AddSingleton<IViewModelNavigationService, ViewModelNavigationService>();
            services.AddSingleton<IAuthenticationCookieStore, AuthenticationCookieStore>();

            services.AddSingleton<ILoginModel, LoginModel>();
            services.AddSingleton<IFriendsModel, FriendsModel>();

            services.AddSingleton<IStateMediator, StateMediator>();           
            
            services.AddTransient<IViewModelGeneratorFactory, ViewModelGeneratorFactory>();
            services.AddTransient<IInstanceDtoFactory, InstanceDtoFactory>();
            services.AddTransient<ILimitedUserDtoFactory, LimitedUserDtoFactory>();
            services.AddTransient<IImageFactory, BitmapFactory>();

            services.AddSingleton<IMainWindowViewModel, MainWindowViewModel>();

            services.AddTransient<IViewModel, LoginUsernamePasswordViewModel>();
            services.AddTransient<IViewModel, LoginOtpViewModel>();
            services.AddTransient<IViewModel, FriendsListViewModel>();

            services.AddTransient<INotifyIconForm, NotifyIconForm>();

            services.AddTransient<InstanceDto>();
        }
    }
}
