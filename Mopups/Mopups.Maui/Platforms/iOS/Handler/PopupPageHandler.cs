using Microsoft.Maui.Handlers;

namespace Mopups.Platforms.iOS
{
    public class PopupPageHandler : PageHandler
    {
        public PopupPageHandler()
        {
            SetMauiContext(MauiUIApplicationDelegate.Current.Application.Windows[0].Handler.MauiContext); //Still a hack?
        }

        public PopupPageHandler(IMauiContext context)
        {
            SetMauiContext(context); //Still a hack?
        }
        protected override Microsoft.Maui.Platform.ContentView CreatePlatformView()
        {
            return base.CreatePlatformView();
        }

        protected override void DisconnectHandler(Microsoft.Maui.Platform.ContentView nativeView)
        {
            base.DisconnectHandler(nativeView);
        }
    }
}
