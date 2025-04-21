using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace VRCFriends.Converters
{
    public class BitmapToBitmapImageConverter : IValueConverter
    {
        public static BitmapImage? ConvertByteArrayToBitMapImage(byte[]? imageByteArray)
        {
            if (imageByteArray != null)
            {
                BitmapImage img = new();

                using (MemoryStream memStream = new(imageByteArray))
                {
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = memStream;
                    img.EndInit();
                    img.Freeze();
                }

                return img;
            }

            return null;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not null)
            {
                ImageConverter converter = new();
                byte[]? imageByteArray = (byte[]?)converter.ConvertTo(value, typeof(byte[]));
                return ConvertByteArrayToBitMapImage(imageByteArray);
            }

            return null;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
