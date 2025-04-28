using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace VRCFriends.UserControls
{
    /// <summary>
    /// Interaction logic for BusyIndicatorControl.xaml
    /// </summary>
    public partial class BusyIndicatorControl : UserControl
    {
        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.RegisterAttached(
                "IsBusy",
                typeof(bool),
                typeof(BusyIndicatorControl),
                new PropertyMetadata(false, new PropertyChangedCallback(OnIsBusyPropertyChangedCallback)));

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.RegisterAttached(
                "MessageText",
                typeof(string),
                typeof(BusyIndicatorControl),
                new PropertyMetadata("", new PropertyChangedCallback(OnMessageTextPropertyChangedCallback)));

        public static bool GetIsBusy(DependencyObject obj) => (bool)obj.GetValue(IsBusyProperty);

        public static void SetIsBusy(DependencyObject obj, bool value) => obj.SetValue(IsBusyProperty, value);

        public static string? GetMessageText(DependencyObject obj) => obj.GetValue(MessageTextProperty) as string;

        public static void SetMessageText(DependencyObject obj, string value) => obj.SetValue(MessageTextProperty, value);

        public BusyIndicatorControl()
        {
            InitializeComponent();
        }

        private static void OnIsBusyPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BusyIndicatorControl userControl)
                userControl.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnMessageTextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BusyIndicatorControl userControl)
                userControl.txtMessageText.Text = e.NewValue as string;
        }
    }
}
