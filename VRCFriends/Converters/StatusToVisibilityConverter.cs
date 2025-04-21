using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VRChat.API.Model;

namespace VRCFriends.Converters
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserStatus status && targetType == typeof(Visibility))
                return status == UserStatus.Offline ? Visibility.Collapsed : Visibility.Visible;

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
