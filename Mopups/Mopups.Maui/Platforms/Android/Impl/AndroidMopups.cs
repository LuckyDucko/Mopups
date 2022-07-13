using Android.Views;
using Android.Widget;
using AsyncAwaitBestPractices;
using Mopups.Interfaces;
using Mopups.Pages;
using Mopups.Services;

namespace Mopups.Droid.Implementation;

public class AndroidMopups : IPopupPlatform
{
    private static FrameLayout? DecoreView => Platform.CurrentActivity?.Window?.DecorView as FrameLayout;

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
            HandleAccessibility();

            page.Parent = MauiApplication.Current.Application.Windows[0].Content as Element;
            var AndroidNativeView = IPopupPlatform.GetOrCreateHandler<PopupPageHandler>(page).PlatformView as Android.Views.View;
            DecoreView?.AddView(AndroidNativeView);

            return PostAsync(AndroidNativeView);

            static void HandleAccessibility()
            {
                Page? mainPage = Application.Current?.MainPage;

                if (mainPage is null)
                {
                    return;
                }

                int navCount = mainPage.Navigation.NavigationStack.Count;
                int modalCount = mainPage.Navigation.ModalStack.Count;

                Android.Views.View? platformMainPage = mainPage.Handler?.PlatformView as Android.Views.View;
                if(platformMainPage is not null)
                {
                    platformMainPage.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
                }

                if (navCount > 0)
                {
                    Android.Views.View? currentPage = mainPage.Navigation?.NavigationStack[navCount - 1]?.Handler?.PlatformView as Android.Views.View;
                    if (currentPage is not null)
                    {
                        currentPage.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
                    }
                }
                
                if (modalCount > 0)
                {
                    Android.Views.View? backgroundModelPage = mainPage.Navigation?.ModalStack[modalCount - 1]?.Handler?.PlatformView as Android.Views.View;
                    if (backgroundModelPage is not null)
                    {
                        backgroundModelPage.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
                    }
                }

                DisableFocusableInTouchMode((mainPage.Handler?.PlatformView as Android.Views.View)?.Parent);
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
            HandleAccessibility();

            DecoreView?.RemoveView(renderer.PlatformView as Android.Views.View);
            renderer.DisconnectHandler(); //?? no clue if works
            page.Parent = null;

            return PostAsync(DecoreView);
        }

        return Task.CompletedTask;

        static void HandleAccessibility()
        {
            Page? mainPage = Application.Current?.MainPage;

            if(mainPage is null)
            {
                return;
            }

            int navCount = mainPage.Navigation.NavigationStack.Count;
            int modalCount = mainPage.Navigation.ModalStack.Count;

            Android.Views.View? platformMainPage = mainPage.Handler?.PlatformView as Android.Views.View;
            if (platformMainPage is not null)
            {
                platformMainPage.ImportantForAccessibility = ImportantForAccessibility.Auto;
            }

            if (navCount > 0)
            {
                Android.Views.View? currentPage = mainPage.Navigation?.NavigationStack[navCount - 1]?.Handler?.PlatformView as Android.Views.View;
                if (currentPage is not null)
                {
                    currentPage.ImportantForAccessibility = ImportantForAccessibility.Auto;
                }
            }

            if (modalCount > 0)
            {
                Android.Views.View? backgroundModelPage = mainPage.Navigation?.ModalStack[modalCount - 1]?.Handler?.PlatformView as Android.Views.View;
                if (backgroundModelPage is not null)
                {
                    backgroundModelPage.ImportantForAccessibility = ImportantForAccessibility.Auto;
                }
            }
        }
    }

    Task<bool> PostAsync(Android.Views.View? nativeView)
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
