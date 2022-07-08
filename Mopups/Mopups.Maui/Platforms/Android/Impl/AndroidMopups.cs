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

            page.Parent = MauiApplication.Current.Application.Windows[0].Content as Element;
            var AndroidNativeView = IPopupPlatform.GetOrCreateHandler<PopupPageHandler>(page).PlatformView as Android.Views.View;
            decoreView?.AddView(AndroidNativeView);
            return PostAsync(AndroidNativeView);
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
            DecoreView?.RemoveView(renderer.PlatformView as Android.Views.View);
            renderer.DisconnectHandler(); //?? no clue if works
            page.Parent = null;

            return PostAsync(DecoreView);
        }

        return Task.CompletedTask;
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
