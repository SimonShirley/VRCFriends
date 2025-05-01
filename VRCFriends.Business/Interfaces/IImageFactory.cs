using System.Drawing;
using System.IO;

namespace VRCFriends.Business.Interfaces
{
    public interface IImageFactory
    {
        Image Create(Stream stream);
        Image Create(string filename);
    }
}
