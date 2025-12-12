using UIKit;

namespace Mopups.Platforms.iOS
{
    public static class MopupsHelper
    {
        public static UIScene FindKeyScene() => FindKeyScene<UIScene>();

        public static T FindKeyScene<T>() where T : UIScene
        {
            return UIApplication.SharedApplication.ConnectedScenes
                .ToArray()
                .OfType<T>()
                .Where(scene => scene.Session.Role == UIWindowSceneSessionRole.Application)
                .FirstOrDefault(scene => scene.ActivationState == UISceneActivationState.ForegroundActive);
        }
        
        public static UIWindow FindKeyWindow()
        {
#if IOS13_0_OR_GREATER
            return FindKeyScene<UIWindowScene>()?.Windows.FirstOrDefault(window => window.IsKeyWindow);
#else
            return UIApplication.SharedApplication.KeyWindow;
#endif
        }
        
        public static UIView FindFirstResponder(this UIView view)
        {
            if (view.IsFirstResponder)
                return view;

            foreach (var subview in view.Subviews)
            {
                var responder = subview.FindFirstResponder();
                if (responder != null)
                    return responder;
            }

            return null;
        }
    }
}