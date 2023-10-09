using Mopups.PreBaked.Interfaces;

using Mopups.Pages;

namespace Mopups.PreBaked.PopupPages.SingleResponse
{
    public partial class SingleResponsePopupPage : PopupPage, IGenericViewModel<SingleResponseViewModel>
    {
        public SingleResponseViewModel ViewModel
        {
            get => BindingContext as SingleResponseViewModel;
            set => BindingContext = value;
        }

        public void SetViewModel(SingleResponseViewModel viewModel) => ViewModel = viewModel;
        public SingleResponseViewModel GetViewModel() => ViewModel;


        public SingleResponsePopupPage()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.SafeCloseModal<SingleResponsePopupPage>();
            return base.OnBackButtonPressed();
        }

        protected override bool OnBackgroundClicked()
        {
            ViewModel.SafeCloseModal<SingleResponsePopupPage>();
            return base.OnBackgroundClicked();
        }

    }
}
