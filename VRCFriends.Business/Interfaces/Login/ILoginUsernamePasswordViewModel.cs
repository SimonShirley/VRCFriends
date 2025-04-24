using System.Threading.Tasks;

namespace VRCFriends.Business.Interfaces.Login
{
    public interface ILoginUsernamePasswordViewModel
    {
        void LoginUser();
        Task LoginUserAsync();
    }
}
