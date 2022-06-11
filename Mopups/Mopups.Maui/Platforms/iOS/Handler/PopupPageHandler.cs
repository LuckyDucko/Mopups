using CoreGraphics;

using Foundation;

using Microsoft.Maui.Handlers;

using Mopups.Pages;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UIKit;

namespace Mopups.Platforms.iOS
{
    public class PopupPageHandler : PageHandler
    {
        private readonly UIGestureRecognizer _tapGestureRecognizer;
        private NSObject? _willChangeFrameNotificationObserver;
        private NSObject? _willHideNotificationObserver;
        private bool _isDisposed;

        internal CGRect KeyboardBounds { get; private set; } = CGRect.Empty;
        internal PopupPage CurrentElement => (PopupPage)VirtualView;


        #region Main Methods

        public PopupPageHandler()
        {
            _tapGestureRecognizer = new UITapGestureRecognizer(OnTap)
            {
                CancelsTouchesInView = false
            };
        }


        protected override Microsoft.Maui.Platform.ContentView CreatePlatformView()
        {
            return base.CreatePlatformView();
        }




        protected override void DisconnectHandler(Microsoft.Maui.Platform.ContentView nativeView)
        {
            nativeView?.RemoveGestureRecognizer(_tapGestureRecognizer);
            base.DisconnectHandler(nativeView);
            _isDisposed = true;
        }
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        View?.RemoveGestureRecognizer(_tapGestureRecognizer);
        //    }

        //    base.Dispose(disposing);

        //    _isDisposed = true;
        //}

        #endregion

        #region Gestures Methods

        private void OnTap(UITapGestureRecognizer e)
        {
            var view = e.View;
            var location = e.LocationInView(view);
            var subview = view.HitTest(location, null);
            if (Equals(subview, view))
            {
                CurrentElement.SendBackgroundClick();
            }
        }

        #endregion

        //#region Life Cycle Methods

        //public override void ViewDidLoad()
        //{
        //    base.ViewDidLoad();

        //    ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
        //    ModalTransitionStyle = UIModalTransitionStyle.CoverVertical;

        //    View?.AddGestureRecognizer(_tapGestureRecognizer);
        //}

        //public override void ViewDidUnload()
        //{
        //    base.ViewDidUnload();

        //    View?.RemoveGestureRecognizer(_tapGestureRecognizer);
        //}

        //public override void ViewWillAppear(bool animated)
        //{
        //    base.ViewWillAppear(animated);

        //    UnregisterAllObservers();

        //    _willChangeFrameNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, KeyBoardUpNotification);
        //    _willHideNotificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, KeyBoardDownNotification);
        //}

        //public override void ViewWillDisappear(bool animated)
        //{
        //    base.ViewWillDisappear(animated);

        //    UnregisterAllObservers();
        //}

        //#endregion

        #region Layout Methods

        //public override void ViewDidLayoutSubviews()
        //{
        //    base.ViewDidLayoutSubviews();
        //    this.UpdateSize();
        //}

        #endregion

        //#region Notifications Methods

        //private void UnregisterAllObservers()
        //{
        //    if (_willChangeFrameNotificationObserver != null)
        //        NSNotificationCenter.DefaultCenter.RemoveObserver(_willChangeFrameNotificationObserver);

        //    if (_willHideNotificationObserver != null)
        //        NSNotificationCenter.DefaultCenter.RemoveObserver(_willHideNotificationObserver);

        //    _willChangeFrameNotificationObserver = null;
        //    _willHideNotificationObserver = null;
        //}

        //private void KeyBoardUpNotification(NSNotification notifi)
        //{
        //    KeyboardBounds = UIKeyboard.BoundsFromNotification(notifi);

        //    ViewDidLayoutSubviews();
        //}

        //private async void KeyBoardDownNotification(NSNotification notifi)
        //{
        //    NSObject duration = null!;
        //    var canAnimated = notifi.UserInfo?.TryGetValue(UIKeyboard.AnimationDurationUserInfoKey, out duration);

        //    KeyboardBounds = CGRect.Empty;

        //    if (canAnimated ?? false)
        //    {
        //        //It is needed that buttons are working when keyboard is opened. See #11
        //        await Task.Delay(70);

        //        if (!_isDisposed)
        //            await UIView.AnimateAsync((double)(NSNumber)duration, OnKeyboardAnimated);
        //    }
        //    else
        //    {
        //        ViewDidLayoutSubviews();
        //    }
        //}

        //#endregion

        //#region Animation Methods

        //private void OnKeyboardAnimated()
        //{
        //    if (_isDisposed)
        //        return;

        //    ViewDidLayoutSubviews();
        //}

        //#endregion
    }

}
