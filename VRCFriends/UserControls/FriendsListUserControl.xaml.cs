using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Windows;
using System.Windows.Input;
using Control = System.Windows.Controls.Control;
using UserControl = System.Windows.Controls.UserControl;

namespace VRCFriends.UserControls
{
    /// <summary>
    /// Interaction logic for FriendsListUserControl.xaml
    /// </summary>
    public partial class FriendsListUserControl : UserControl, IDisposable
    {
        private readonly ICommand? _contentVisibilityCommand;

        public static readonly DependencyProperty GroupHeaderProperty =
            DependencyProperty.RegisterAttached(
                "GroupHeader",
                typeof(string),
                typeof(FriendsListUserControl),
                new PropertyMetadata("", new PropertyChangedCallback(OnGroupHeaderPropertyChangedCallback)));

        public static readonly DependencyProperty FriendsCollectionProperty =
            DependencyProperty.RegisterAttached(
                "FriendsCollection",
                typeof(IEnumerable),
                typeof(FriendsListUserControl),
                new PropertyMetadata(null, new PropertyChangedCallback(OnFriendsCollectionPropertyChanged)));

        public static readonly DependencyProperty ShowFriendsListProperty =
            DependencyProperty.RegisterAttached(
                "ShowFriendsList",
                typeof(bool),
                typeof(FriendsListUserControl),
                new PropertyMetadata(true, new PropertyChangedCallback(OnShowFriendsListPropertyChanged)));

        public static string? GetGroupHeader(DependencyObject obj) => obj.GetValue(GroupHeaderProperty) as string;

        public static void SetGroupHeader(DependencyObject obj, string value) => obj.SetValue(GroupHeaderProperty, value);

        public static IEnumerable? GetFriendsCollection(DependencyObject obj) => obj.GetValue(FriendsCollectionProperty) as IEnumerable;

        public static void SetFriendsCollection(DependencyObject obj, IEnumerable? value) => obj.SetValue(FriendsCollectionProperty, value);

        public static bool GetShowFriendsList(DependencyObject obj) => (bool)obj.GetValue(ShowFriendsListProperty);

        public static void SetShowFriendsList(DependencyObject obj, bool value) => obj.SetValue(ShowFriendsListProperty, value);

        public FriendsListUserControl()
        {
            InitializeComponent();

            _contentVisibilityCommand = new RelayCommand(() => { listViewFriendsCollection.Visibility = listViewFriendsCollection.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible; });

            btnGroupHeader.Command = _contentVisibilityCommand;

            listViewFriendsCollection.PreviewMouseWheel += new MouseWheelEventHandler(HandlePreviewMouseWheel);
        }

        private static void OnGroupHeaderPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FriendsListUserControl userControl)
                userControl.txtGroupHeader.Text = e.NewValue as string;
        }

        private static void OnFriendsCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FriendsListUserControl userControl)
                userControl.listViewFriendsCollection.ItemsSource = e.NewValue as IEnumerable;
        }

        private static void OnShowFriendsListPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FriendsListUserControl userControl)
                userControl.listViewFriendsCollection.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HandlePreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;

                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = MouseWheelEvent,
                    Source = sender
                };

                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            listViewFriendsCollection.PreviewMouseWheel -= new MouseWheelEventHandler(HandlePreviewMouseWheel);
        }
    }
}
