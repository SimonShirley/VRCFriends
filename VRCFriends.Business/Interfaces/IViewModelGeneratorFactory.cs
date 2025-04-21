using System.Collections.Generic;

namespace VRCFriends.Business.Interfaces
{
    public interface IViewModelGeneratorFactory
    {
        IEnumerable<IViewModel> ViewModels { get; }

        IViewModel GetViewModel<TViewModel>();
    }
}
