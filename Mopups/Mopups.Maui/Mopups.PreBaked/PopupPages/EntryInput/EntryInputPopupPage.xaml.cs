using Mopups.PreBaked.Interfaces;
using Mopups.PreBaked.PopupPages.EntryInput;

using Mopups.Pages;
namespace Mopups.PreBaked.PopupPages.EntryInput
{
    public partial class EntryInputPopupPage : PopupPage, IGenericViewModel<EntryInputViewModel>
    {
        public EntryInputViewModel ViewModel
        {
            get => BindingContext as EntryInputViewModel;
            set => BindingContext = value;
        }


        public EntryInputPopupPage()
        {
            InitializeComponent();
        }

        public EntryInputViewModel GetViewModel() => ViewModel;
        public void SetViewModel(EntryInputViewModel viewModel) => ViewModel = viewModel;


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }


        protected override bool OnBackButtonPressed()
        {
            ViewModel.SafeCloseModal<EntryInputPopupPage>();
            return base.OnBackButtonPressed();
        }

    }
}
