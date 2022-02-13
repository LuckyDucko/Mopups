
using Mopups.Services;

using System;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;
namespace Mopups.Pages
{
    public partial class PopupPage : ContentPage
    {

        public event EventHandler? BackgroundClicked;

        public static readonly BindableProperty CloseWhenBackgroundIsClickedProperty = BindableProperty.Create(nameof(CloseWhenBackgroundIsClicked), typeof(bool), typeof(PopupPage), true);

        public bool CloseWhenBackgroundIsClicked
        {
            get { return (bool)GetValue(CloseWhenBackgroundIsClickedProperty); }
            set { SetValue(CloseWhenBackgroundIsClickedProperty, value); }
        }

        public static readonly BindableProperty BackgroundInputTransparentProperty = BindableProperty.Create(nameof(BackgroundInputTransparent), typeof(bool), typeof(PopupPage), false);

        public bool BackgroundInputTransparent
        {
            get { return (bool)GetValue(BackgroundInputTransparentProperty); }
            set { SetValue(BackgroundInputTransparentProperty, value); }
        }

        public static readonly BindableProperty HasKeyboardOffsetProperty = BindableProperty.Create(nameof(HasKeyboardOffset), typeof(bool), typeof(PopupPage), true);

        public bool HasKeyboardOffset
        {
            get { return (bool)GetValue(HasKeyboardOffsetProperty); }
            set { SetValue(HasKeyboardOffsetProperty, value); }
        }

        public static readonly BindableProperty KeyboardOffsetProperty = BindableProperty.Create(nameof(KeyboardOffset), typeof(double), typeof(PopupPage), 0d, BindingMode.OneWayToSource);

        public double KeyboardOffset
        {
            get { return (double)GetValue(KeyboardOffsetProperty); }
            private set { SetValue(KeyboardOffsetProperty, value); }
        }



        public PopupPage()
        {
            BackgroundColor = Color.FromArgb("#80000000");
        }

        protected override bool OnBackButtonPressed()
        {
            return false;
        }


        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            height -= KeyboardOffset;
            base.LayoutChildren(x, y, width, height);
        }


        protected virtual bool OnBackgroundClicked()
        {
            return CloseWhenBackgroundIsClicked;
        }


        internal void SendBackgroundClick()
        {
            BackgroundClicked?.Invoke(this, EventArgs.Empty);
            if (OnBackgroundClicked())
            {
                MopupService.Instance.RemovePageAsync(this).SafeFireAndForget();
            }
        }
    }
}
