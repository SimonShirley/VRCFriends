namespace VRCFriends.ViewModels
{
    internal class NotifyIconViewModel : ViewModelBase, IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);


        }
    }
}
