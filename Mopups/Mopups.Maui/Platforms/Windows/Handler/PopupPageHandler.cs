using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Platform;

namespace Mopups.Platforms.Windows
{
    public class PopupPageHandler : PageHandler
    {
        public PopupPageHandler()
        {
        }

        protected override void ConnectHandler(ContentPanel platformView)
        {
            base.ConnectHandler(platformView);

            PlatformView.SizeChanged += (_, e) => VirtualView.ComputeDesiredSize(e.NewSize.Width, e.NewSize.Height);
        }

        protected override ContentPanel CreatePlatformView()
        {
            return new PopupPageRenderer(this);
        }
    }
}
