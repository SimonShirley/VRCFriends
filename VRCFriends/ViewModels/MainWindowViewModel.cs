using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using VRCFriends.Business.Interfaces;

namespace VRCFriends.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IDisposable
    {
        private readonly IMainWindowView _MainWindowView;
        private readonly IStateMediator _StateMediator;

        [ObservableProperty]
        private IViewModel? _currentViewModel;

        public MainWindowViewModel(IMainWindowView mainWindowView, IStateMediator stateMediator)
        {
            _MainWindowView = mainWindowView;
            _MainWindowView.DataContext = this;

            _StateMediator = stateMediator;
            _StateMediator.CurrentViewModelChanged += StateMediator_CurrentViewModelChanged;
            _StateMediator.ShowFriendsContextMenuItemClicked += StateMediator_ShowFriendsContextMenuItemClicked;
        }

        private void StateMediator_ShowFriendsContextMenuItemClicked()
        {
            _MainWindowView.CurrentWindowState = (int)WindowState.Normal;
            _MainWindowView.Activate();

            OnPropertyChanged(nameof(WindowState));
        }

        private void StateMediator_CurrentViewModelChanged() => CurrentViewModel = _StateMediator.CurrentViewModel;

        public void ShowWindow() => _MainWindowView.Show();

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _StateMediator.CurrentViewModelChanged -= StateMediator_CurrentViewModelChanged;
            _StateMediator.ShowFriendsContextMenuItemClicked -= StateMediator_ShowFriendsContextMenuItemClicked;
        }
    }
}
