using Microsoft.Maui.Platform;
using Windows.UI.ViewManagement;
using Rect = Windows.Foundation.Rect;
using Size = Windows.Foundation.Size;
using Mopups.Pages;
using WinPopup = global::Microsoft.UI.Xaml.Controls.Primitives.Popup;
using Microsoft.UI.Xaml.Input;

namespace Mopups.Platforms.Windows
{
    public class PopupPageRenderer : ContentPanel
    {
        private Rect _keyboardBounds;
        private readonly PopupPageHandler handler;

        internal WinPopup? Container { get; private set; }

        private PopupPage CurrentElement => (PopupPage)handler.VirtualView;

        public PopupPageRenderer(PopupPageHandler handler)
        {
            this.handler = handler;
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnKeyboardHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            _keyboardBounds = Rect.Empty;
            UpdateElementSize();
        }

        private void OnKeyboardShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            _keyboardBounds = sender.OccludedRect;
            UpdateElementSize();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateElementSize();

            return base.ArrangeOverride(finalSize);
        }

        internal void Prepare(WinPopup container)
        {
            Container = container;
            UpdateElementSize();

            // Not sure off hand the replacement on these 
            //var inputPane = InputPane.GetForCurrentView();
            //inputPane.Showing += OnKeyboardShowing;
            //inputPane.Hiding += OnKeyboardHiding;

        }


        private void OnUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            DeviceDisplay.Current.MainDisplayInfoChanged -= OnDisplayInfoChanged;
            PointerPressed -= OnBackgroundClick;
        }

        private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            DeviceDisplay.Current.MainDisplayInfoChanged += OnDisplayInfoChanged;
            PointerPressed += OnBackgroundClick;
        }

        private void OnDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            UpdateElementSize();
        }

        internal void Destroy()
        {
            Container = null;

            DeviceDisplay.Current.MainDisplayInfoChanged -= OnDisplayInfoChanged;
            PointerPressed -= OnBackgroundClick;

            // Not sure off hand the replacement on these 
            //var inputPane = InputPane.GetForCurrentView();
            //inputPane.Showing -= OnKeyboardShowing;
            //inputPane.Hiding -= OnKeyboardHiding;

            PointerPressed -= OnBackgroundClick;
        }

        private void OnBackgroundClick(object sender, PointerRoutedEventArgs e)
        {
            if ((e.OriginalSource as PopupPageRenderer) == this)
            {
                CurrentElement.SendBackgroundClick();
            }
        }

        private void UpdateElementSize()
        {
            if (CurrentElement != null)
            {
                var capturedElement = CurrentElement;

                //Window.Current.Bounds replacement
                var platformWindow = handler.MauiContext.Services.GetService<Microsoft.UI.Xaml.Window>();
                var windowBound = platformWindow.Bounds;


                //var visibleBounds = ApplicationView.GetForCurrentView().VisibleBounds;
                // Need to locate the replacement for VisibleBounds
                var visibleBounds = windowBound;

                var keyboardHeight = _keyboardBounds != Rect.Empty ? _keyboardBounds.Height : 0;

                var top = Math.Max(0, visibleBounds.Top - windowBound.Top);
                var bottom = Math.Max(0, windowBound.Bottom - visibleBounds.Bottom);
                var left = Math.Max(0, visibleBounds.Left - windowBound.Left);
                var right = Math.Max(0, windowBound.Right - visibleBounds.Right);

                var systemPadding = new Thickness(left, top, right, bottom);

                capturedElement.SetValue(PopupPage.SystemPaddingProperty, systemPadding);
                capturedElement.SetValue(PopupPage.KeyboardOffsetProperty, keyboardHeight);
                capturedElement.HeightRequest = windowBound.Height;
                capturedElement.WidthRequest = windowBound.Width;

                //(capturedElement as IView).Measure(windowBound.Width, windowBound.Height);
                //(capturedElement as IView).Arrange(new Microsoft.Maui.Graphics.Rect(windowBound.X, windowBound.Y, windowBound.Width, windowBound.Height));

                //if its not invoked on MainThread when the popup is showed it will be blank until the user manually resizes of owner window
                //this.DispatcherQueue.TryEnqueue(() =>
                //{
                //    capturedElement.Layout(new Microsoft.Maui.Graphics.Rect(windowBound.X, windowBound.Y, windowBound.Width, windowBound.Height));
                //    capturedElement.ForceLayout();
                //});
            }
        }
    }
}
