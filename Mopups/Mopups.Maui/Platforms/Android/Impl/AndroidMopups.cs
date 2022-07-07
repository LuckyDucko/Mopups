using Android.Views;
using Android.Widget;

using AsyncAwaitBestPractices;
using Mopups.Interfaces;

using Mopups.Pages;
using Mopups.Services;

namespace Mopups.Droid.Implementation;

public class AndroidMopups : IPopupPlatform
{
    private static FrameLayout? DecoreView => Platform.CurrentActivity.Window.DecorView as FrameLayout;

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

    public Task AddAsync(PopupPage page)
    {
        try
        {
            var decoreView = DecoreView;

            HandleAccessibilityWorkaround();

            page.Parent = MauiApplication.Current.Application.Windows[0].Content as Element;
            var AndroidNativeView = IPopupPlatform.GetOrCreateHandler<PopupPageHandler>(page).PlatformView as Android.Views.View;
            decoreView?.AddView(AndroidNativeView);

            return PostAsync(AndroidNativeView);

            static void HandleAccessibilityWorkaround()
            {
                int? navCount = Application.Current?.MainPage?.Navigation.NavigationStack.Count;
                int? modalCount = Application.Current?.MainPage?.Navigation.ModalStack.Count;
                
                if (navCount is not null)
                {
                    Android.Views.View? backgroundPage = navCount == 0
                        ? Application.Current?.MainPage?.Handler?.PlatformView as Android.Views.View
                        : Application.Current?.MainPage?.Navigation?.NavigationStack[(int)navCount - 1]?.Handler?.PlatformView as Android.Views.View;
                    
                    if (backgroundPage is not null)
                    {
                        backgroundPage.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
                    }
                }
                
                if (modalCount is not null && modalCount > 0)
                {
                    Android.Views.View? backgroundModelPage = Application.Current?.MainPage?.Navigation.ModalStack[(int)modalCount - 1]?.Handler?.PlatformView as Android.Views.View;

                    if (backgroundModelPage is not null)
                    {
                        backgroundModelPage.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
                    }
                }

                DisableFocusableInTouchMode((Application.Current?.MainPage?.Handler?.PlatformView as Android.Views.View)?.Parent);
            }

            static void DisableFocusableInTouchMode(IViewParent? parent)
            {
                var view = parent;
                string className = $"{view?.GetType().Name}";

                while (!className.Contains("PlatformRenderer") && view != null)
                {
                    view = view.Parent;
                    className = $"{view?.GetType().Name}";
                }

                if (view is Android.Views.View androidView)
                {
                    androidView.Focusable = false;
                    androidView.FocusableInTouchMode = false;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task RemoveAsync(PopupPage page)
    {
        var renderer = IPopupPlatform.GetOrCreateHandler<PopupPageHandler>(page);
        if (renderer != null)
        {
            HandleAccessibilityWorkaround();

            DecoreView?.RemoveView(renderer.PlatformView as Android.Views.View);
            renderer.DisconnectHandler(); //?? no clue if works
            page.Parent = null;

            return PostAsync(DecoreView);
        }

        return Task.CompletedTask;

        static void HandleAccessibilityWorkaround()
        {
            var mainPage = Application.Current?.MainPage;

            if(mainPage is null)
            {
                return;
            }

            var navCount = mainPage.Navigation.NavigationStack.Count;
            var modalCount = mainPage.Navigation.ModalStack.Count;

            var mainPageRenderer = mainPage.Handler?.PlatformView as Android.Views.View;

            // Workaround for https://github.com/rotorgames/Rg.Plugins.Popup/issues/721
            if (!(mainPage is MultiPage<Page>) && mainPageRenderer is not null)
            {
                mainPageRenderer.ImportantForAccessibility = ImportantForAccessibility.Auto;
            }

            Android.Views.View? backgroundPage = navCount == 0
                       ? Application.Current?.MainPage?.Handler?.PlatformView as Android.Views.View
                       : Application.Current?.MainPage?.Navigation?.NavigationStack[navCount - 1]?.Handler?.PlatformView as Android.Views.View;

            if (backgroundPage is not null)
            {
                backgroundPage.ImportantForAccessibility = ImportantForAccessibility.Auto;
            }
            
            if (modalCount > 0)
            {
                Android.Views.View? backgroundModelPage = Application.Current?.MainPage?.Navigation.ModalStack[(int)modalCount - 1]?.Handler?.PlatformView as Android.Views.View;

                if (backgroundModelPage is not null)
                {
                    backgroundModelPage.ImportantForAccessibility = ImportantForAccessibility.Auto;
                }
            }
        }
    }

    Task<bool> PostAsync(Android.Views.View nativeView)
    {
        if (nativeView == null)
        {
            return Task.FromResult(true);
        }
        var tcs = new TaskCompletionSource<bool>();

        nativeView.Post(() => tcs.SetResult(true));

        return tcs.Task;
    }
}
