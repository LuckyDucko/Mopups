using CoreGraphics;

using Foundation;
using Mopups.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIKit;

namespace Mopups.Platforms.iOS
{
    internal class PopupPageRenderer : UIViewController
    {
        private PopupPageHandler? _renderer;
        private readonly UIGestureRecognizer _tapGestureRecognizer;
        private NSObject? _willChangeFrameNotificationObserver;
        private NSObject? _willHideNotificationObserver;
        private bool _isDisposed;

        internal CGRect KeyboardBounds { get; private set; } = CGRect.Empty;

        public PopupPageHandler? Handler => _renderer;

        public PopupPageRenderer(PopupPageHandler handler)
        {
            _renderer = handler;

            _tapGestureRecognizer = new UITapGestureRecognizer(OnTap)
            {
                CancelsTouchesInView = false
            };
        }

        public PopupPageRenderer(IntPtr handle) : base(handle)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderer.ViewController.View?.RemoveGestureRecognizer(_tapGestureRecognizer);
                _renderer = null; 
            }

            base.Dispose(disposing);
            _isDisposed = true;
        }


        private void OnTap(UITapGestureRecognizer e)
        {
            var view = e.View.Subviews.First();
            var location = e.LocationInView(view);
            var subview = view.HitTest(location, null);

            if (Equals(subview, view))
            {
                ((PopupPage)Handler.VirtualView).SendBackgroundClick();
            }
        }


        public override bool ShouldAutomaticallyForwardRotationMethods => true;

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            UpdateSize(this);
            PresentedViewController?.ViewDidLayoutSubviews();

            void UpdateSize(PopupPageRenderer handler)
            {
                var currentElement = ((PopupPage)Handler.VirtualView);

                if (handler.Handler.PlatformView?.Superview?.Frame == null || currentElement == null)
                    return;

                var superviewFrame = handler.Handler.PlatformView.Superview.Frame;
                var applicationFrame = UIScreen.MainScreen.ApplicationFrame;
                var keyboardOffset = 0d;

#if NET8_0_OR_GREATER
                if (handler.KeyboardBounds.Height > 0)
                {
                    var keyWindow = MopupsHelper.FindKeyWindow();
                    var firstResponder = keyWindow?.FindFirstResponder();
                    
                    if (firstResponder != null)
                    {
                        // getting first responder position on the whole screen
                        var firstResponderScreenFrame = firstResponder.ConvertRectToView(firstResponder.Bounds, null);
                        
                        // proceed if bottom part of the first responder is lower than keyboard
                        // adding bottom safe area inset because OS returns us shifted up pos of the first responder
                        if (firstResponderScreenFrame.Bottom + keyWindow.SafeAreaInsets.Bottom > handler.KeyboardBounds.Top)
                        {
                            keyboardOffset = firstResponder.Frame.Height;
                        }
                    }
                }
#else
                keyboardOffset = handler.KeyboardBounds.Height;
#endif

                var systemPadding = new Thickness
                {
                    Left = applicationFrame.Left,
                    Top = applicationFrame.Top,
                    Right = applicationFrame.Right - applicationFrame.Width - applicationFrame.Left,
                    Bottom = applicationFrame.Bottom - applicationFrame.Height - applicationFrame.Top
                };

                if ((handler.Handler.VirtualView.Width != superviewFrame.Width && handler.Handler.VirtualView.Height != superviewFrame.Height)
                    || currentElement.SystemPadding.Bottom != systemPadding.Bottom
                    || currentElement.KeyboardOffset != keyboardOffset)
                {
                    currentElement.BatchBegin();
                    currentElement.SystemPadding = systemPadding;
                    currentElement.KeyboardOffset = keyboardOffset;
                    currentElement.Layout(new Rect(currentElement.X, currentElement.Y, superviewFrame.Width, superviewFrame.Height));
                    currentElement.BatchCommit();
                }
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
            ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;

            _renderer.ViewController.View?.AddGestureRecognizer(_tapGestureRecognizer);
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            _renderer.ViewController.View?.RemoveGestureRecognizer(_tapGestureRecognizer);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UnregisterAllObservers();

            _willChangeFrameNotificationObserver = UIKeyboard.Notifications.ObserveWillShow((sender, args) =>
            {
                KeyboardBounds = args.FrameEnd;
                ViewDidLayoutSubviews();
            });

            _willHideNotificationObserver = UIKeyboard.Notifications.ObserveWillHide(async (sender, args) =>
            {
                KeyboardBounds = CGRect.Empty;

                if (args.AnimationDuration > 0.01)
                {
                    if (!_isDisposed)
                        await UIView.AnimateAsync(args.AnimationDuration, OnKeyboardAnimated);
                }
                else
                {
                    ViewDidLayoutSubviews();
                }

                void OnKeyboardAnimated()
                {
                    if (_isDisposed)
                        return;

                    ViewDidLayoutSubviews();
                }
            });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UnregisterAllObservers();
        }

        private void UnregisterAllObservers()
        {
            
            _willChangeFrameNotificationObserver?.Dispose();
            _willHideNotificationObserver?.Dispose();

            _willChangeFrameNotificationObserver = null;
            _willHideNotificationObserver = null;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            if ((ChildViewControllers != null) && (ChildViewControllers.Length > 0))
            {
                return ChildViewControllers[0].GetSupportedInterfaceOrientations();
            }
            return base.GetSupportedInterfaceOrientations();
        }

        public override UIInterfaceOrientation PreferredInterfaceOrientationForPresentation()
        {
            if ((ChildViewControllers != null) && (ChildViewControllers.Length > 0))
            {
                return ChildViewControllers[0].PreferredInterfaceOrientationForPresentation();
            }
            return base.PreferredInterfaceOrientationForPresentation();
        }

        public override UIViewController ChildViewControllerForStatusBarHidden()
        {
            return _renderer?.ViewController!;
        }

        public override bool PrefersStatusBarHidden()
        {
            return _renderer?.ViewController.PrefersStatusBarHidden() ?? false;
        }

        public override UIViewController ChildViewControllerForStatusBarStyle()
        {
            return _renderer?.ViewController!;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return (UIStatusBarStyle)(_renderer?.ViewController.PreferredStatusBarStyle())!;
        }

        public override bool ShouldAutorotate()
        {
            if ((ChildViewControllers != null) && (ChildViewControllers.Length > 0))
            {
                return ChildViewControllers[0].ShouldAutorotate();
            }
            return base.ShouldAutorotate();
        }

        public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
        {
            if ((ChildViewControllers != null) && (ChildViewControllers.Length > 0))
            {
                return ChildViewControllers[0].ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
            }
            return base.ShouldAutorotateToInterfaceOrientation(toInterfaceOrientation);
        }
    }
}
