using VRCFriends.Business.Interfaces;

namespace VRCFriends.Factories
{
    public class ViewModelGeneratorFactory : IViewModelGeneratorFactory
    {
        private readonly IEnumerable<IViewModel>? _viewModels;

        public IEnumerable<IViewModel>? ViewModels => _viewModels;

        public ViewModelGeneratorFactory(IEnumerable<IViewModel>? viewModels)
        {
            _viewModels = viewModels;
        }

        public IViewModel? GetViewModel<TViewModel>()
        {
            return _viewModels?.FirstOrDefault(vm => vm is TViewModel viewModel);
        }
    }
}
