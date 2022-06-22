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

        public PopupPageHandler()
        {
            this.SetMauiContext(MauiUIApplicationDelegate.Current.Application.Windows[0].Handler.MauiContext); //Still a hack?
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
