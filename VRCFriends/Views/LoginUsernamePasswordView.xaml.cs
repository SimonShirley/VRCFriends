using System.Windows;
using System.Windows.Controls;
using VRCFriends.Business.Interfaces.Login;
using VRCFriends.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace VRCFriends.Views
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    /// 
    public partial class LoginUsernamePasswordView : UserControl, ILoginUsernamePasswordView
    {

        public LoginUsernamePasswordView()
        {
            InitializeComponent();
        }

        private void VrcPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && DataContext is LoginUsernamePasswordViewModel viewModel)
                viewModel.VrcPassword = passwordBox.SecurePassword;
        }
    }
}
