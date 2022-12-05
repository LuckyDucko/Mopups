using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Mopups.Platforms.Windows
{
    public class PopupPageHandler : PageHandler
    {
        public PopupPageHandler()
        {
        }

        protected override ContentPanel CreatePlatformView()
        {
            return new PopupPageRenderer(this);
        }
    }
}
