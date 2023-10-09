using Mopups.PreBaked.AbstractClasses;

namespace Mopups.PreBaked.Interfaces
{
    public interface IGenericViewModel<TViewModel> where TViewModel : BasePopupViewModel
    {
        void SetViewModel(TViewModel viewModel);
        TViewModel GetViewModel();
    }
}