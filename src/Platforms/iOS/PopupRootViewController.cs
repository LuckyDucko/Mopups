using CoreGraphics;
using Foundation;
using Nkraft.Mopups.Pages;
using UIKit;

namespace Nkraft.Mopups.Platforms.iOS;

internal sealed class PopupRootViewController : UIViewController
{
    private readonly WeakReference<UIWindow>? _hostWindow;

    private PopupPage? _page;
    private UIViewController? _pageController;
    private UITapGestureRecognizer? _backgroundTap;
    private NSObject? _keyboardFrameObserver;
    private NSObject? _keyboardHideObserver;
    private nfloat _keyboardOverlap;

    public PopupRootViewController(PopupPage page, UIViewController pageController, UIWindow? hostWindow)
    {
        _page = page;
        _pageController = pageController;
        _hostWindow = hostWindow is null ? null : new WeakReference<UIWindow>(hostWindow);
    }

    private UIViewController? HostRootController =>
        _hostWindow is not null && _hostWindow.TryGetTarget(out var window) ? window.RootViewController : null;

    public override void LoadView()
    {
        View = new UIView { BackgroundColor = UIColor.Clear };
    }

    /// <summary>
    /// Embeds the page's controller as a child, reports the safe area and keyboard to the page, and turns taps on
    /// the empty area around the content into background clicks.
    /// </summary>
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        if (_pageController?.View is not { } pageView || View is null)
            return;

        AddChildViewController(_pageController);
        pageView.Frame = View.Bounds;
        pageView.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
        View.AddSubview(pageView);
        _pageController.DidMoveToParentViewController(this);

        // With BackgroundInputTransparent, PopupWindow.HitTest reports the tap and this recognizer never sees it.
        _backgroundTap = new UITapGestureRecognizer(HandleBackgroundTap)
        {
            CancelsTouchesInView = false,
            ShouldReceiveTouch = (_, touch) => touch.View is { } view && IsBackgroundView(view),
        };
        View.AddGestureRecognizer(_backgroundTap);

        // WillChangeFrame covers show, hide, undock and size changes (e.g. the QuickType bar).
        _keyboardFrameObserver = UIKeyboard.Notifications.ObserveWillChangeFrame(
            (_, args) => UpdateKeyboardOverlap(args.FrameEnd));
        _keyboardHideObserver = UIKeyboard.Notifications.ObserveWillHide(
            (_, _) => UpdateKeyboardOverlap(CGRect.Empty));
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        ReportInsets();
    }

    public override void ViewSafeAreaInsetsDidChange()
    {
        base.ViewSafeAreaInsetsDidChange();
        ReportInsets();
    }

    // Status bar appearance follows the popup page.
    public override UIViewController? ChildViewControllerForStatusBarStyle() => _pageController;

    public override UIViewController? ChildViewControllerForStatusBarHidden() => _pageController;

    // Orientation follows the app window, so a popup never rotates an app that locks orientation in code.
    public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations() =>
        HostRootController?.GetSupportedInterfaceOrientations() ?? base.GetSupportedInterfaceOrientations();

    /// <summary>True for the empty area around the popup content.</summary>
    internal bool IsBackgroundView(UIView view) =>
        ReferenceEquals(view, ViewIfLoaded)
        || ReferenceEquals(view, _pageController?.ViewIfLoaded)
        || ReferenceEquals(view, _page?.Handler?.PlatformView as UIView);

    /// <summary>Takes the page controller out of the hierarchy. Safe to call more than once.</summary>
    internal void DetachPage()
    {
        _keyboardFrameObserver?.Dispose();
        _keyboardHideObserver?.Dispose();
        _keyboardFrameObserver = null;
        _keyboardHideObserver = null;

        if (_backgroundTap is not null)
        {
            ViewIfLoaded?.RemoveGestureRecognizer(_backgroundTap);
            _backgroundTap.Dispose();
            _backgroundTap = null;
        }

        if (_pageController is not null)
        {
            _pageController.WillMoveToParentViewController(null);
            _pageController.ViewIfLoaded?.RemoveFromSuperview();
            _pageController.RemoveFromParentViewController();
            _pageController = null;
        }

        _page = null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DetachPage();
        }

        base.Dispose(disposing);
    }

    private void HandleBackgroundTap()
    {
        _page?.SendBackgroundClick();
    }

    private void UpdateKeyboardOverlap(CGRect keyboardFrameInScreen)
    {
        if (ViewIfLoaded is not { } view)
            return;

        nfloat overlap = 0;
        if (!keyboardFrameInScreen.IsEmpty && view.Window?.WindowScene?.Screen is { } screen)
        {
            var keyboardInView = view.ConvertRectFromCoordinateSpace(keyboardFrameInScreen, screen.CoordinateSpace);
            var intersection = CGRect.Intersect(view.Bounds, keyboardInView);
            overlap = intersection.IsEmpty ? 0 : intersection.Height;
        }

        if (overlap == _keyboardOverlap)
            return;

        _keyboardOverlap = overlap;
        ReportInsets();
    }

    private void ReportInsets()
    {
        if (_page is null || ViewIfLoaded is not { } view)
            return;

        var insets = view.SafeAreaInsets;
        _page.UpdateInsets(new Thickness(insets.Left, insets.Top, insets.Right, insets.Bottom), _keyboardOverlap);
    }
}
