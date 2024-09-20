namespace Mopups.PreBaked.Interfaces
{
    public interface IGenericViewModel<TViewModel> where TViewModel : IBasePopupViewModel
    {
        void SetViewModel(TViewModel viewModel);
        TViewModel GetViewModel();
    }
}