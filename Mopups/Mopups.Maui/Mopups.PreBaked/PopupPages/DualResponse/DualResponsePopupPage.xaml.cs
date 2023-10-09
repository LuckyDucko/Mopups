
using Mopups.PreBaked.Interfaces;

using Mopups.Pages;

namespace Mopups.PreBaked.PopupPages.DualResponse
{
    public partial class DualResponsePopupPage : PopupPage, IGenericViewModel<DualResponseViewModel>
    {
        public DualResponseViewModel ViewModel
        {
            get => BindingContext as DualResponseViewModel;
            set => BindingContext = value;
        }

        public void SetViewModel(DualResponseViewModel viewModel) => ViewModel = viewModel;
        public DualResponseViewModel GetViewModel() => ViewModel;

        public DualResponsePopupPage()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.SafeCloseModal<DualResponsePopupPage>();
            return base.OnBackButtonPressed();
        }

        protected override bool OnBackgroundClicked()
        {
            ViewModel.SafeCloseModal<DualResponsePopupPage>();
            return base.OnBackgroundClicked();
        }
    }
}