using Mopups.PreBaked.Interfaces;

using Mopups.Pages;
namespace Mopups.PreBaked.PopupPages.Login
{
    public partial class LoginPopupPage : PopupPage, IGenericViewModel<LoginViewModel>
    {
        public LoginViewModel ViewModel
        {
            get => BindingContext as LoginViewModel;
            set => BindingContext = value;
        }

        public LoginPopupPage()
        {
            InitializeComponent();
        }

        public LoginViewModel GetViewModel() => ViewModel;
        public void SetViewModel(LoginViewModel viewModel) => ViewModel = viewModel;

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.SafeCloseModal<LoginPopupPage>();
            return base.OnBackButtonPressed();
        }

        protected override bool OnBackgroundClicked()
        {
            ViewModel.SafeCloseModal<LoginPopupPage>();
            return base.OnBackgroundClicked();
        }
    }
}
