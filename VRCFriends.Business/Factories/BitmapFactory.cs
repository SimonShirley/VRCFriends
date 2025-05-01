using System.Drawing;
using System.IO;
using VRCFriends.Business.Interfaces;

namespace VRCFriends.Business.Factories
{
    public class BitmapFactory : IImageFactory
    {
        public Image Create(Stream stream) => new Bitmap(stream);
        public Image Create(string fileName) => new Bitmap(fileName);
    }
}
