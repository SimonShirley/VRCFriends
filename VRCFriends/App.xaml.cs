using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Windows;
using VRCFriends.Business.Interfaces;
using VRCFriends.Services;
using VRChat.API.Client;
using Application = System.Windows.Application;

namespace VRCFriends
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IDisposable
    {
        IServiceProvider? _serviceProvider;
        IStateMediator? _stateMediator;

        protected override void OnStartup(StartupEventArgs e)
        {
            ServiceCollection services = new();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();

            RunApp();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddVRChatApiServices();
            services.AddVRCFriendsServices();            
        }
        
        private void RunApp()
        {
            if (_serviceProvider is null)
                Shutdown(1);

            _ = _serviceProvider!.GetRequiredService<INotifyIconForm>();

            GlobalConfiguration.Instance = Configuration.MergeConfigurations(GlobalConfiguration.Instance, new Configuration { UserAgent = "VRCFriends/0.0.1 email_at_simonshirley.uk" });

            var authenticationCookieStore = _serviceProvider!.GetRequiredService<IAuthenticationCookieStore>();
            authenticationCookieStore.LoadAuthenticationCookies(GlobalConfiguration.Instance);

            var mainWindowViewModel = _serviceProvider!.GetRequiredService<IMainWindowViewModel>();

            // required to load the main window
            _ = _serviceProvider!.GetRequiredService<IViewModelNavigationService>();

            _stateMediator = _serviceProvider!.GetRequiredService<IStateMediator>();
            _stateMediator.AppExited += StateMediator_AppExited;
            _stateMediator.OnAppStarted();

            mainWindowViewModel.ShowWindow();
        }

        private void StateMediator_AppExited() => Shutdown(0);

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (_stateMediator is not null)
                _stateMediator.AppExited -= StateMediator_AppExited;
        }
    }
}
