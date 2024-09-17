using CoreGraphics;

using Mopups.Pages;

using UIKit;

namespace Mopups.Platforms.iOS
{
    internal class PopupWindow : UIWindow
    {

        private bool _stop = false;
        public PopupWindow(IntPtr handle) : base(handle)
        {
        }

        public PopupWindow()
        {

        }

        public PopupWindow(UIWindowScene uiWindowScene) : base(uiWindowScene)
        {

        }

         public override UIView? HitTest(CGPoint point, UIEvent? uievent)
        {
            try
            {
                var platformHandler = (PopupPageRenderer?)RootViewController;
                var renderer = platformHandler?.Handler;
                var hitTestResult = base.HitTest(point, uievent);

                if(renderer?.VirtualView is null)
                {
                    return hitTestResult;
                }

                if(renderer.VirtualView is not PopupPage formsElement)
                    return hitTestResult;

                if(formsElement.InputTransparent)
                    return null;

                if(formsElement.BackgroundInputTransparent && renderer.PlatformView == hitTestResult)
                {
                    formsElement.SendBackgroundClick();
                    return null;
                }
                return hitTestResult;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}
