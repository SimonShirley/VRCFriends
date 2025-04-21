namespace VRCFriends.Business.Interfaces
{
    public interface IMainWindowView
    {
        object DataContext { get; set; }

        int CurrentWindowState { get; set; }

        void Show();

        bool Activate();
    }
}
