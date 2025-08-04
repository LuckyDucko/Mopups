using AsyncAwaitBestPractices;
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

        public static bool SendBackPressed(Action? backPressedHandler = null)
        {
            var popupNavigationInstance = MopupService.Instance;

            if (popupNavigationInstance.PopupStack.Count > 0)
            {
                var lastPage = popupNavigationInstance.PopupStack[popupNavigationInstance.PopupStack.Count - 1];

                var isPreventClose = lastPage.SendBackButtonPressed();

                if (!isPreventClose)
                {
                    popupNavigationInstance.PopAsync().SafeFireAndForget();
                }

                return true;
            }

            backPressedHandler?.Invoke();

            return false;
        }

        public async Task AddAsync(PopupPage page)
        {
            var mainPage = Application.Current.MainPage;
            mainPage.AddLogicalChild(page);

            var popup = new global::Microsoft.UI.Xaml.Controls.Primitives.Popup();

            // Use TOPLATFORM to create your handlers
            // I'd recommend wiring up all your services through ConfigureMopups
            // builder.Services.AddScoped<IPopupPlatform, PopupPlatform>();
            // builder.Services.AddScoped<IPopupNavigation, PopupNavigation>();
            // Then you can use contructor resolution instead of singletons
            // But I figured we could do that in a later PR and just work on windows here

            var renderer = (PopupPageRenderer)page.ToPlatform(mainPage.Handler.MauiContext);
            renderer.Prepare(popup);
            popup.Child = renderer;

            // https://github.com/microsoft/microsoft-ui-xaml/issues/3389
            popup.XamlRoot = mainPage.Handler.MauiContext.Services.GetService<Microsoft.UI.Xaml.Window>().Content.XamlRoot;
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
                page.Parent?.RemoveLogicalChild(page);
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
