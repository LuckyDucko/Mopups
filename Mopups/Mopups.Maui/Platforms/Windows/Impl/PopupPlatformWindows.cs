
using Microsoft.Maui.Platform;
using Mopups.Interfaces;
using Mopups.Pages;
using Mopups.Platforms.Windows;
using Mopups.Services;

namespace Mopups.Windows.Implementation
{
    class PopupPlatformWindows : IPopupPlatform
    {
        private IPopupNavigation PopupNavigationInstance => MopupService.Instance;

        //public event EventHandler OnInitialized
        //{
        //    add => Popup.OnInitialized += value;
        //    remove => Popup.OnInitialized -= value;
        //}

        //public bool IsInitialized => Popup.IsInitialized;

        public bool IsSystemAnimationEnabled => true;

        public PopupPlatformWindows()
        {
            //SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        // Probably wire this through the life cycle parats like you have with android
        //private async void OnBackRequested(object sender, BackRequestedEventArgs e)
        //{
        //    var lastPopupPage = PopupNavigationInstance.PopupStack.LastOrDefault();

        //    if (lastPopupPage != null)
        //    {
        //        var isPrevent = lastPopupPage.DisappearingTransactionTask != null || lastPopupPage.SendBackButtonPressed();

        //        if (!isPrevent)
        //        {
        //            e.Handled = true;
        //            await PopupNavigationInstance.PopAsync();
        //        }
        //    }
        //}

        public async Task AddAsync(PopupPage page)
        {
            page.Parent = Application.Current.MainPage;

            var popup = new global::Microsoft.UI.Xaml.Controls.Primitives.Popup();

            // Use TOPLATFORM to create your handlers
            // I'd recommend wiring up all your services through ConfigureMopups
            // builder.Services.AddScoped<IPopupPlatform, PopupPlatform>();
            // builder.Services.AddScoped<IPopupNavigation, PopupNavigation>();
            // Then you can use contructor resolution instead of singletons
            // But I figured we could do that in a later PR and just work on windows here

            var renderer = (PopupPageRenderer)page.ToPlatform(Application.Current.MainPage.Handler.MauiContext);

            renderer.Prepare(popup);
            popup.Child = renderer;


            // https://github.com/microsoft/microsoft-ui-xaml/issues/3389
            popup.XamlRoot = 
                Application.Current.MainPage.Handler.MauiContext.Services.GetService<Microsoft.UI.Xaml.Window>().Content.XamlRoot;

            popup.IsOpen = true;
            page.ForceLayout();

            await Task.Delay(5);
        }

        public async Task RemoveAsync(PopupPage page)
        {
            if (page == null)
                throw new Exception("Popup page is null");

            var renderer = (PopupPageRenderer)page.ToPlatform(Application.Current.MainPage.Handler.MauiContext);
            var popup = renderer.Container;

            if (popup != null)
            {
                renderer.Destroy();

                Cleanup(page);
                page.Parent = null;
                popup.Child = null;
                popup.IsOpen = false;
            }

            await Task.Delay(5);
        }

        internal static void Cleanup(VisualElement element)
        {
            element.Handler?.DisconnectHandler();
        }
    }
}
