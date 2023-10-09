using System;
using System.Collections.Generic;

using Mopups.PreBaked.Interfaces;

using Mopups.Pages;

namespace Mopups.PreBaked.PopupPages.Loader
{
    public partial class LoaderPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>
    {
        public LoaderPopupPage()
        {
            InitializeComponent();
        }

        public LoaderViewModel ViewModel
        {
            get => BindingContext as LoaderViewModel;
            set => BindingContext = value;
        }

        public void SetViewModel(LoaderViewModel viewModel) => ViewModel = viewModel;
        public LoaderViewModel GetViewModel() => ViewModel;

        protected override bool OnBackButtonPressed()
        {
            ViewModel.SafeCloseModal<LoaderPopupPage>();
            return base.OnBackButtonPressed();
        }

        protected override bool OnBackgroundClicked()
        {
            ViewModel.SafeCloseModal<LoaderPopupPage>();
            return base.OnBackgroundClicked();
        }

    }
}
