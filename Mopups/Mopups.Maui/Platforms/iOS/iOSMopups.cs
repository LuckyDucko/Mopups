using Mopups.Interfaces;
using Mopups.Pages;
using Mopups.Platforms.iOS;

using UIKit;
namespace Mopups.iOS.Implementation;

internal class iOSMopups : IPopupPlatform
{
    // It's necessary because GC in Xamarin.iOS 13 removes all UIWindow if there are not any references to them. See #459
    private readonly List<UIWindow> _windows = new List<UIWindow>();


    private static bool IsiOS13OrNewer => UIDevice.CurrentDevice.CheckSystemVersion(13, 0);


    public Task AddAsync(PopupPage page)
    {
        var mainPage = Application.Current.MainPage;
        mainPage.AddLogicalChild(page);

        var keyWindow = GetKeyWindow(UIApplication.SharedApplication);
        if (keyWindow?.WindowLevel == UIWindowLevel.Normal)
            keyWindow.WindowLevel = -1;

        var handler = (page.Handler ??= new PopupPageHandler(page.Parent.Handler.MauiContext)) as PopupPageHandler;

        PopupWindow window;

        if (IsiOS13OrNewer)
        {
            var connectedScene = UIApplication.SharedApplication.ConnectedScenes.ToArray()
                .Where(scene => scene.Session.Role == UIWindowSceneSessionRole.Application)
                .FirstOrDefault(x => x.ActivationState == UISceneActivationState.ForegroundActive);

            if (connectedScene != null && connectedScene is UIWindowScene windowScene)
                window = new PopupWindow(windowScene);
            else
                window = new PopupWindow();

            _windows.Add(window);
        }
        else
            window = new PopupWindow();

        window.BackgroundColor = UIColor.Clear;
        window.RootViewController = new PopupPageRenderer(handler);

        if (window.RootViewController.View != null)
            window.RootViewController.View.BackgroundColor = UIColor.Clear;

        window.WindowLevel = UIWindowLevel.Normal;
        window.MakeKeyAndVisible();

        handler.ViewController.ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
        handler.ViewController.ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;


        return window.RootViewController.PresentViewControllerAsync(handler.ViewController, false);

        UIWindow GetKeyWindow(UIApplication application)
        {
            if (!IsiOS13OrNewer)
                return UIApplication.SharedApplication.KeyWindow;

            var window = application
                .ConnectedScenes
                .ToArray()
                .OfType<UIWindowScene>()
                .Where(scene => scene.Session.Role == UIWindowSceneSessionRole.Application)
                .SelectMany(scene => scene.Windows)
                .FirstOrDefault(window => window.IsKeyWindow);

            return window;
        }
    }

    public async Task RemoveAsync(PopupPage page)
    {
        if (page == null)
            throw new Exception("Popup page is null");

        var handler = page.Handler as PopupPageHandler;
        var viewController = handler?.ViewController;

        await Task.Delay(50);

        if (handler != null && viewController != null && !viewController.IsBeingDismissed)
        {
            var window = viewController.View?.Window;
            page.Parent?.RemoveLogicalChild(page);

            if (window != null)
            {
                var rvc = window.RootViewController;

                if (rvc != null)
                {
                    await rvc.DismissViewControllerAsync(false);
                    DisposeModelAndChildrenHandlers(page);
                    rvc.Dispose();
                }

                window.RootViewController = null;
                window.Hidden = true;

                if (IsiOS13OrNewer && _windows.Contains(window))
                    _windows.Remove(window);

                window.Dispose();
                window = null;
            }

            if (_windows.Count > 0)
                _windows.Last().WindowLevel = UIWindowLevel.Normal;
            else
                RestoreLoweredWindows();
        }
    }

    // AddAsync lowered the app's key window to -1. Restore every lowered window rather than asking for
    // the key window, which can be null straight after the popup window (the key window) is hidden.
    private static void RestoreLoweredWindows()
    {
        var windows = OperatingSystem.IsIOSVersionAtLeast(13)
            ? UIApplication.SharedApplication.ConnectedScenes.ToArray().OfType<UIWindowScene>().SelectMany(scene => scene.Windows)
            : UIApplication.SharedApplication.Windows;

        foreach (var window in windows.Where(window => window.WindowLevel == -1))
            window.WindowLevel = UIWindowLevel.Normal;
    }

    private static void DisposeModelAndChildrenHandlers(VisualElement view)
    {
        foreach (var descendant in view.GetVisualTreeDescendants())
        {
            if (descendant is IElement child)
            {
                IElementHandler handler = child.Handler;
                child?.Handler?.DisconnectHandler();
                (handler?.PlatformView as UIView)?.RemoveFromSuperview();
                (handler?.PlatformView as UIView)?.Dispose();
            }
        }

        view?.Handler?.DisconnectHandler();
        (view?.Handler?.PlatformView as UIView)?.RemoveFromSuperview();
        (view?.Handler?.PlatformView as UIView)?.Dispose();
    }
}
