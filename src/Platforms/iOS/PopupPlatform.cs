using Microsoft.Maui.Platform;
using Nkraft.Mopups.Pages;
using Nkraft.Mopups.Platforms.iOS;
using UIKit;

namespace Nkraft.Mopups.Services;

internal sealed partial class PopupPlatform
{
    private readonly Dictionary<PopupPage, PopupWindow> _windows = [];
    private readonly List<PopupWindow> _order = [];

    public partial Window? HostWindow()
    {
        var windows = Application.Current?.Windows;
        if (windows is null || windows.Count == 0)
            return null;

        // A popup window may be key right now, so fall back to the foreground scene's window.
        return windows.FirstOrDefault(window => PlatformWindow(window)?.IsKeyWindow == true)
            ?? windows.FirstOrDefault(window =>
                PlatformWindow(window)?.WindowScene?.ActivationState == UISceneActivationState.ForegroundActive)
            ?? windows[0];
    }

    /// <summary>
    /// Hosts each popup in its own <see cref="PopupWindow"/> stacked above the app window, with the page's view
    /// controller embedded as a child of <see cref="PopupRootViewController"/>: no modal presentation, so no
    /// present/dismiss dance and no artificial delays. The windows are held strongly, since UIKit tears down windows
    /// nobody holds on to (upstream issue 459), in stacking order, topmost last.
    /// </summary>
    public partial Task AddAsync(PopupPage page, Window host, PopupPage? below)
    {
        if (_windows.ContainsKey(page))
            return Task.CompletedTask;

        var context = host.Handler?.MauiContext
            ?? throw new InvalidOperationException("The host window has no MauiContext.");
        var hostWindow = PlatformWindow(host);
        var scene = hostWindow?.WindowScene ?? ForegroundScene()
            ?? throw new InvalidOperationException("There is no foreground scene to show the popup in.");

        var handler = (IPlatformViewHandler)page.ToHandler(context);
        var pageController = handler.ViewController
            ?? throw new InvalidOperationException("The popup page's handler has no view controller.");

        var window = new PopupWindow(scene, page, hostWindow)
        {
            BackgroundColor = UIColor.Clear,
            WindowLevel = UIWindowLevel.Normal + _order.Count + 1,
            RootViewController = new PopupRootViewController(page, pageController, hostWindow),
        };

        _windows[page] = window;
        _order.Add(window);

        window.MakeKeyAndVisible();

        // Lays the page out now, so it has its size and insets before the appearing animation reads them.
        window.RootViewController.View?.LayoutIfNeeded();

        return Task.CompletedTask;
    }

    public partial Task RemoveAsync(PopupPage page)
    {
        if (!_windows.Remove(page, out var window))
            return Task.CompletedTask;

        _order.Remove(window);
        var hostWindow = window.HostWindow;

        // Taking the page's view out of the window is what makes MAUI raise Page.Unloaded.
        if (window.RootViewController is PopupRootViewController root)
        {
            root.DetachPage();
            window.RootViewController = null;
            root.Dispose();
        }

        // Hidden and let go, not disposed: a third party that got the key window may still hold it
        // (upstream issue 83).
        window.Hidden = true;
        window.Forget();

        if (_order.Count > 0)
        {
            _order[^1].MakeKeyWindow();
        }
        else
        {
            hostWindow?.MakeKeyWindow();
        }

        return Task.CompletedTask;
    }

    private static UIWindow? PlatformWindow(Window window) => window.Handler?.PlatformView as UIWindow;

    private static UIWindowScene? ForegroundScene() =>
        UIApplication.SharedApplication.ConnectedScenes
            .ToArray()
            .OfType<UIWindowScene>()
            .Where(scene => scene.Session.Role == UIWindowSceneSessionRole.Application)
            .OrderByDescending(scene => scene.ActivationState == UISceneActivationState.ForegroundActive)
            .FirstOrDefault();
}
