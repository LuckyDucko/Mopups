using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Platform;
using Nkraft.Mopups.Pages;
using AView = Android.Views.View;

namespace Nkraft.Mopups.Platforms.Android;

internal sealed class PopupViewGroup : ContentViewGroup
{
    private readonly GestureDetector _taps;
    private bool _disposed;

    public PopupPage? Page { get; set; }

    public PopupViewGroup(Context context) : base(context)
    {
        _taps = new GestureDetector(context, new TapListener(this));
    }

    /// <summary>
    /// The popup covers the whole window; its touches end here, as background clicks when they land outside the
    /// content, or go through to the window under it when the page is
    /// <see cref="PopupPage.BackgroundInputTransparent"/>.
    /// </summary>
    public override bool DispatchTouchEvent(MotionEvent? e)
    {
        if (_disposed)
            return false;

        var handled = base.DispatchTouchEvent(e);

        // A popup swallows every touch its content didn't take, so nothing under it is touched. Claiming the touch
        // also keeps the rest of the gesture coming, which the tap detector needs.
        return Page is { BackgroundInputTransparent: true } ? handled : true;
    }

    public override bool OnTouchEvent(MotionEvent? e)
    {
        if (_disposed || e is null)
            return false;

        if (Page is { BackgroundInputTransparent: true } page)
        {
            // Not claimed, so the touch goes on to the window under the popup; that leaves only its start to react to.
            if (e.ActionMasked == MotionEventActions.Down && !IsOnContent(e))
            {
                page.SendBackgroundClick();
            }

            return false;
        }

        _taps.OnTouchEvent(e);
        return base.OnTouchEvent(e);
    }

    public override WindowInsets? OnApplyWindowInsets(WindowInsets? insets)
    {
        if (insets is not null)
        {
            ReportInsets(WindowInsetsCompat.ToWindowInsetsCompat(insets, this));
        }

        return base.OnApplyWindowInsets(insets);
    }

    protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
    {
        // Only the base's arrange: arranging or forcing the page's layout from here re-enters it and overflows the
        // stack (upstream issue 171).
        base.OnLayout(changed, left, top, right, bottom);
        if (ViewCompat.GetRootWindowInsets(this) is { } insets)
        {
            ReportInsets(insets);
        }
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();
        HideKeyboard();
    }

    protected override void OnDetachedFromWindow()
    {
        // After the removal that detached this view has finished, so the keyboard doesn't reopen for the content.
        Post(HideKeyboard);
        base.OnDetachedFromWindow();
    }

    protected override void OnWindowVisibilityChanged(ViewStates visibility)
    {
        base.OnWindowVisibilityChanged(visibility);

        // The popup's size wasn't updated on Android 7 and later without it (upstream issue 209).
        if (visibility == ViewStates.Visible)
        {
            RequestLayout();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _disposed = true;
            _taps.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// The system bars and cutout as the page's padding, and the keyboard's height as its offset; neither counts the
    /// other, so the page can take the larger of the two at the bottom.
    /// </summary>
    private void ReportInsets(WindowInsetsCompat insets)
    {
        if (Page is not { } page)
            return;

        var bars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());
        var keyboard = insets.GetInsets(WindowInsetsCompat.Type.Ime());
        var padding = new Thickness(
            Context.FromPixels(bars.Left),
            Context.FromPixels(bars.Top),
            Context.FromPixels(bars.Right),
            Context.FromPixels(bars.Bottom));

        page.UpdateInsets(padding, Context.FromPixels(keyboard.Bottom));
    }

    private void HideKeyboard()
    {
        if (Platform.CurrentActivity?.Window?.DecorView is { } decorView)
        {
            Context.HideKeyboard(decorView);
        }
    }

    private bool IsOnContent(MotionEvent e)
    {
        if (GetChildAt(0) is not { } content)
            return false;

        var location = new int[2];
        content.GetLocationOnScreen(location);
        return e.RawX > location[0] && e.RawX < location[0] + content.Width
            && e.RawY > location[1] && e.RawY < location[1] + content.Height;
    }

    private sealed class TapListener(PopupViewGroup view) : GestureDetector.SimpleOnGestureListener
    {
        private readonly PopupViewGroup _view = view;

        public override bool OnSingleTapUp(MotionEvent? e)
        {
            if (e is not null && !_view.IsOnContent(e))
            {
                _view.Page?.SendBackgroundClick();
            }

            return false;
        }
    }
}
