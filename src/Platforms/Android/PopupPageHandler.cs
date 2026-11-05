using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Platforms.Android;

internal sealed class PopupPageHandler : PageHandler
{
    protected override ContentViewGroup CreatePlatformView() =>
        new PopupViewGroup(Context) { CrossPlatformLayout = VirtualView };

    protected override void ConnectHandler(ContentViewGroup platformView)
    {
        base.ConnectHandler(platformView);
        ((PopupViewGroup)platformView).Page = VirtualView as PopupPage;
    }

    protected override void DisconnectHandler(ContentViewGroup platformView)
    {
        ((PopupViewGroup)platformView).Page = null;
        base.DisconnectHandler(platformView);
    }
}
