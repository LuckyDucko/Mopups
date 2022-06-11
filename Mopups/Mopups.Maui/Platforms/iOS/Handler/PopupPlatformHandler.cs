using CoreGraphics;

using Foundation;

using Mopups.Platforms.iOS.Extentions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIKit;

namespace Mopups.Platforms.iOS
{
    internal class PopupPlatformHandler : UIViewController
    {
        private PopupPageHandler? _handler;
        private readonly UIGestureRecognizer _tapGestureRecognizer;
        private NSObject? _willChangeFrameNotificationObserver;
        private NSObject? _willHideNotificationObserver;
        private bool _isDisposed;

        internal CGRect KeyboardBounds { get; private set; } = CGRect.Empty;

        public PopupPageHandler? Handler => _handler;

        public PopupPlatformHandler(PopupPageHandler handler)
        {
            _handler = handler;
        }

        public PopupPlatformHandler(IntPtr handle) : base(handle)
        {
            // Fix #307
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _handler = null;
            }

            base.Dispose(disposing);
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
            return _handler?.ViewController!;
        }

        public override bool PrefersStatusBarHidden()
        {
            return _handler?.ViewController.PrefersStatusBarHidden() ?? false;
        }

        public override UIViewController ChildViewControllerForStatusBarStyle()
        {
            return _handler?.ViewController!;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return (UIStatusBarStyle)(_handler?.ViewController.PreferredStatusBarStyle())!;
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

        public override bool ShouldAutomaticallyForwardRotationMethods => true;

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            this.UpdateSize();
            PresentedViewController?.ViewDidLayoutSubviews();
        }

        #region Life Cycle Methods

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

            _willChangeFrameNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, KeyBoardUpNotification);
            _willHideNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyBoardDownNotification);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            UnregisterAllObservers();
        }

        #endregion

        #region Notifications Methods

        private void UnregisterAllObservers()
        {
            if (_willChangeFrameNotificationObserver != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_willChangeFrameNotificationObserver);

            if (_willHideNotificationObserver != null)
                NSNotificationCenter.DefaultCenter.RemoveObserver(_willHideNotificationObserver);

            _willChangeFrameNotificationObserver = null;
            _willHideNotificationObserver = null;
        }

        private void KeyBoardUpNotification(NSNotification notifi)
        {
            //KeyboardBounds = UIKeyboard.BoundsFromNotification(notifi);
            //KeyboardBounds = (CGRect)notifi.UserInfo.ValueForKey(new NSString("UIKeyboardFrameBeginUserInfoKey")) ;

            ViewDidLayoutSubviews();
        }

        private async void KeyBoardDownNotification(NSNotification notifi)
        {
            NSObject duration = null!;
            var canAnimated = notifi.UserInfo?.TryGetValue(UIKeyboard.AnimationDurationUserInfoKey, out duration);

            KeyboardBounds = CGRect.Empty;

            if (canAnimated ?? false)
            {
                //It is needed that buttons are working when keyboard is opened. See #11
                await Task.Delay(70);

                if (!_isDisposed)
                    await UIView.AnimateAsync((double)(NSNumber)duration, OnKeyboardAnimated);
            }
            else
            {
                ViewDidLayoutSubviews();
            }
        }

        #endregion
        #region Animation Methods

        private void OnKeyboardAnimated()
        {
            if (_isDisposed)
                return;

            ViewDidLayoutSubviews();
        }

        #endregion
    }

}
