using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VRChat.API.Model;
using Brush = System.Windows.Media.Brush;

namespace VRCFriends.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserStatus status && targetType == typeof(Brush))
            {
                return status switch
                {
                    UserStatus.Active => new SolidColorBrush(Colors.Green),
                    UserStatus.JoinMe => new SolidColorBrush(Colors.Green),
                    UserStatus.AskMe => new SolidColorBrush(Colors.Orange),                    
                    UserStatus.Busy => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
