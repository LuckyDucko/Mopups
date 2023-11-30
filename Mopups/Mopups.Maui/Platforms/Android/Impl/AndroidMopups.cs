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
        HandleAccessibility(true, page.DisableAndroidAccessibilityHandling);

        page.Parent = MauiApplication.Current.Application.Windows[0].Content as Element;
        var AndroidNativeView = IPopupPlatform.GetOrCreateHandler<PopupPageHandler>(page).PlatformView as Android.Views.View;
        DecoreView?.AddView(AndroidNativeView);

        return PostAsync(AndroidNativeView);
    }

    public Task RemoveAsync(PopupPage page)
    {
        var renderer = IPopupPlatform.GetOrCreateHandler<PopupPageHandler>(page);

        if (renderer != null)
        {
            HandleAccessibility(false, page.DisableAndroidAccessibilityHandling);

            DecoreView?.RemoveView(renderer.PlatformView as Android.Views.View);
            renderer.DisconnectHandler(); //?? no clue if works
            page.Parent = null;

            return PostAsync(DecoreView);
        }

        return Task.CompletedTask;
    }

    //! important keeps reference to pages that accessibility has applied to. This is so accessibility can be removed properly when popup is removed. #https://github.com/LuckyDucko/Mopups/issues/93
    readonly List<Android.Views.View?> views = new();
    void HandleAccessibility(bool showPopup, bool disableAccessibilityHandling)
    {
        if (disableAccessibilityHandling)
        {
            return;
        }

        if (showPopup)
        {
            Page? mainPage = Application.Current?.MainPage;

            if (mainPage is null)
            {
                return;
            }

            views.Add(mainPage.Handler?.PlatformView as Android.Views.View);

            int navCount = mainPage.Navigation.NavigationStack.Count;
            int modalCount = mainPage.Navigation.ModalStack.Count;

            if (navCount > 0)
            {
                views.Add(mainPage.Navigation?.NavigationStack[navCount - 1]?.Handler?.PlatformView as Android.Views.View);
            }

            if (modalCount > 0)
            {
                views.Add(mainPage.Navigation?.ModalStack[modalCount - 1]?.Handler?.PlatformView as Android.Views.View);
            }
        }

        foreach (var view in views)
        {
            ProcessView(showPopup, view);
        }

        static void ProcessView(bool showPopup, Android.Views.View? view)
        {
            if (view is null)
            {
                return;
            }

            // Screen reader
            view.ImportantForAccessibility = showPopup ? ImportantForAccessibility.NoHideDescendants : ImportantForAccessibility.Auto;

            // Keyboard navigation
            ((ViewGroup)view).DescendantFocusability = showPopup ? DescendantFocusability.BlockDescendants : DescendantFocusability.AfterDescendants;
            view.ClearFocus();
        }
    }

    static Task<bool> PostAsync(Android.Views.View? nativeView)
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
