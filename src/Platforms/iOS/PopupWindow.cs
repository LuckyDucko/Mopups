using CoreGraphics;
using Nkraft.Mopups.Pages;
using UIKit;

namespace Nkraft.Mopups.Platforms.iOS;

internal sealed class PopupWindow : UIWindow
{
    private PopupPage? _page;
    private double _lastBackgroundTouchTimestamp = -1;

    public PopupWindow(UIWindowScene scene, PopupPage page, UIWindow? hostWindow) : base(scene)
    {
        _page = page;
        HostWindow = hostWindow;
    }

    public UIWindow? HostWindow { get; private set; }

    public override UIView? HitTest(CGPoint point, UIEvent? uievent)
    {
        var hit = base.HitTest(point, uievent);
        if (hit is null || _page is not { } page)
            return hit;

        // Null lets the touch fall through to the windows below.
        if (page.InputTransparent)
            return null;

        if (page.BackgroundInputTransparent && IsBackground(hit))
        {
            // UIKit hit-tests several times for one touch, and for pointer hover on iPad: once per touch event.
            if (uievent is { Type: UIEventType.Touches } && uievent.Timestamp != _lastBackgroundTouchTimestamp)
            {
                _lastBackgroundTouchTimestamp = uievent.Timestamp;
                page.SendBackgroundClick();
            }

            return null;
        }

        return hit;
    }

    /// <summary>Drops the page and the host window once the popup is gone, without disposing the window.</summary>
    public void Forget()
    {
        _page = null;
        HostWindow = null;
    }

    private bool IsBackground(UIView hit) =>
        ReferenceEquals(hit, this)
        || ReferenceEquals(hit, RootViewController?.View)
        || (RootViewController is PopupRootViewController root && root.IsBackgroundView(hit));
}
