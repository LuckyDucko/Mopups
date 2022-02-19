using Android.Content;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Mopups.Droid.Gestures;

namespace Mopups.Pages;

public class PopupPageHandler : ContentViewHandler
{
    private readonly MopupGestureDetectorListener _gestureDetectorListener;
    private readonly GestureDetector _gestureDetector;
    private DateTime _downTime;
    private Point _downPosition;
    private bool _disposed;

    public PopupPageHandler()
    {
        try
        {
            //--HACK--
            this.SetMauiContext(new MauiContext(MauiApplication.Current.Services, MauiApplication.Current.ApplicationContext));
            //
            _gestureDetectorListener = new MopupGestureDetectorListener();

            _gestureDetectorListener.Clicked += OnBackgroundClick;

            _gestureDetector = new GestureDetector(MauiApplication.Current.ApplicationContext, _gestureDetectorListener);
            ForceHandlerPauseWaitForVirtualView();

        }
        catch (Exception)
        {
            throw;
        }

        //--HACK--
        void ForceHandlerPauseWaitForVirtualView()
        {
            Task.Run(async () =>
            {
                while (this.VirtualView == null)
                {
                    await Task.Delay(100);
                }
                this.NativeView.LayoutChange += PopupPage_LayoutChange;
                this.NativeView.Touch += NativeView_Touch;
                this.NativeView.Touch += NativeView_Touch1;
            });
        }
    }

    private void NativeView_Touch1(object? sender, Android.Views.View.TouchEventArgs e)
    {
        OnTouchEvent(sender, e.Event);
    }


    private bool OnTouchEvent(object? sender, MotionEvent? e)
    {
        if (_disposed)
        {
            return false;
        }

        var baseValue = (sender as Android.Views.View).OnTouchEvent(e);

        _gestureDetector.OnTouchEvent(e);

        if ((sender as PopupPage)?.BackgroundInputTransparent == true)
        {
            OnBackgroundClick(sender, e);
        }

        return false;
    }

    private void OnBackgroundClick(object? sender, MotionEvent e)
    {
        var isInRegion = IsInRegion(e.RawX, e.RawY, (sender as Android.Views.View)!);

        if (!isInRegion)
            (sender as PopupPage).SendBackgroundClick();
    }

    // Fix for "CloseWhenBackgroundIsClicked not works on Android with Xamarin.Forms 2.4.0.280" #173
    private bool IsInRegion(float x, float y, Android.Views.View v)
    {
        var mCoordBuffer = new int[2];

        v.GetLocationOnScreen(mCoordBuffer);
        return mCoordBuffer[0] + v.Width > x &&    // right edge
               mCoordBuffer[1] + v.Height > y &&   // bottom edge
               mCoordBuffer[0] < x &&              // left edge
               mCoordBuffer[1] < y;                // top edge
    }

    private void NativeView_Touch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        try
        {
            DispatchTouchEvent(e.Event);

            void DispatchTouchEvent(MotionEvent e)
            {

                if (e.Action == MotionEventActions.Down)
                {
                    _downTime = DateTime.UtcNow;
                    _downPosition = new Point(e.RawX, e.RawY);
                }
                if (e.Action != MotionEventActions.Up)
                {
                    return;
                }

                if (_disposed)
                    return;

                Android.Views.View? currentFocus1 = Platform.CurrentActivity.CurrentFocus;

                if (currentFocus1 is Android.Widget.EditText)
                {
                    Android.Views.View? currentFocus2 = Platform.CurrentActivity.CurrentFocus;
                    if (currentFocus1 == currentFocus2 && _downPosition.Distance(new(e.RawX, e.RawY)) <= Context.ToPixels(20.0) && !(DateTime.UtcNow - _downTime > TimeSpan.FromMilliseconds(200.0)))
                    {
                        int[] location = new int[2];
                        currentFocus1.GetLocationOnScreen(location);
                        float num1 = e.RawX + currentFocus1.Left - location[0];
                        float num2 = e.RawY + currentFocus1.Top - location[1];
                        if (!new Rectangle(currentFocus1.Left, currentFocus1.Top, currentFocus1.Width, currentFocus1.Height).Contains(num1, num2))
                        {
                            Context.HideKeyboard(currentFocus1);
                            currentFocus1.ClearFocus();
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void PopupPage_LayoutChange(object? sender, Android.Views.View.LayoutChangeEventArgs e)
    {
        try
        {
            var activity = Platform.CurrentActivity;

            Thickness systemPadding;
            var keyboardOffset = 0d;

            var decoreView = activity.Window.DecorView;
            var decoreHeight = decoreView.Height;
            var decoreWidth = decoreView.Width;

            using var visibleRect = new Android.Graphics.Rect();

            decoreView.GetWindowVisibleDisplayFrame(visibleRect);

            using var screenSize = new Android.Graphics.Point();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {

                var windowInsets = activity.WindowManager.DefaultDisplay.Cutout;


                var bottomPadding = windowInsets.SafeInsetBottom;

                if (screenSize.Y - visibleRect.Bottom > bottomPadding)
                {
                    keyboardOffset = Context.FromPixels(screenSize.Y - visibleRect.Bottom);
                }

                systemPadding = new Microsoft.Maui.Thickness
                {
                    Left = Context.FromPixels(windowInsets.SafeInsetLeft),
                    Top = Context.FromPixels(windowInsets.SafeInsetTop),
                    Right = Context.FromPixels(windowInsets.SafeInsetRight),
                    Bottom = Context.FromPixels(bottomPadding)
                };
            }
            else
            {
                var keyboardHeight = 0d;

                if (visibleRect.Bottom < screenSize.Y)
                {
                    keyboardHeight = screenSize.Y - visibleRect.Bottom;
                    keyboardOffset = Context.FromPixels(decoreHeight - visibleRect.Bottom);
                }

                systemPadding = new Microsoft.Maui.Thickness
                {
                    Left = Context.FromPixels(visibleRect.Left),
                    Top = Context.FromPixels(visibleRect.Top),
                    Right = Context.FromPixels(decoreWidth - visibleRect.Right),
                    Bottom = Context.FromPixels(decoreHeight - visibleRect.Bottom - keyboardHeight)
                };
            }


            //CurrentElement.SetValue(PopupPage.SystemPaddingProperty, systemPadding);
            //CurrentElement.SetValue(PopupPage.KeyboardOffsetProperty, keyboardOffset);
            this.NativeView.Layout((int)Context.FromPixels(e.Left), (int)Context.FromPixels(e.Top), (int)Context.FromPixels(e.Right), (int)Context.FromPixels(e.Bottom));
            this.NativeView.ForceLayout();

            //base.OnLayout(changed, l, t, r, b);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
