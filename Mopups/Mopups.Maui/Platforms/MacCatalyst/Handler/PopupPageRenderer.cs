using CoreGraphics;

using Foundation;
using Mopups.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIKit;

namespace Mopups.Platforms.MacCatalyst
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
                _renderer = null; 
                View?.RemoveGestureRecognizer(_tapGestureRecognizer);
            }

            base.Dispose(disposing);
            _isDisposed = true;
        }


        private void OnTap(UITapGestureRecognizer e)
        {
            var view = e.View;
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

                var systemPadding = new Thickness
                {
                    Left = applicationFrame.Left,
                    Top = applicationFrame.Top,
                    Right = applicationFrame.Right - applicationFrame.Width - applicationFrame.Left,
                    Bottom = applicationFrame.Bottom - applicationFrame.Height - applicationFrame.Top + handler.KeyboardBounds.Height
                };

                if ((handler.Handler.VirtualView.Width != superviewFrame.Width && handler.Handler.VirtualView.Height != superviewFrame.Height)
                    || currentElement.SystemPadding.Bottom != systemPadding.Bottom)
                {
                    currentElement.BatchBegin();
                    currentElement.SystemPadding = systemPadding;
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

            View?.AddGestureRecognizer(_tapGestureRecognizer);
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            View?.RemoveGestureRecognizer(_tapGestureRecognizer);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            UnregisterAllObservers();

            _willChangeFrameNotificationObserver = UIKeyboard.Notifications.ObserveWillShow((sender, args) =>
            {
                KeyboardBounds = args.FrameBegin;
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
