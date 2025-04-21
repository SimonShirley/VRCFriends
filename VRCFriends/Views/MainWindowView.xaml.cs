using System.Windows;
using VRCFriends.Business.Interfaces;

namespace VRCFriends.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window, IMainWindowView, IDisposable
    {
        public MainWindowView()
        {
            InitializeComponent();

            StateChanged += MainWindowView_StateChanged;
        }

        private void MainWindowView_StateChanged(object? sender, EventArgs e)
        {
            if (sender is Window window)
                window.ShowInTaskbar = window.WindowState != WindowState.Minimized;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            StateChanged -= MainWindowView_StateChanged;
        }

        public int CurrentWindowState
        {
            get => (int)WindowState;
            set {
                WindowState = (WindowState)value;
            }
        }
    }
}