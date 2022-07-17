using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Mopups.Platforms.Android.Renderers;

namespace Mopups.Pages;

public class PopupPageHandler : PageHandler
{
    public bool _disposed;

    public PopupPageHandler()
    {
        this.SetMauiContext(MauiApplication.Current.Application.Windows[0].Handler.MauiContext);
    }

    protected override void ConnectHandler(ContentViewGroup platformView)
    {
        (platformView as PopupPageRenderer).PopupHandler = this;
        base.ConnectHandler(platformView);
    }

    protected override ContentViewGroup CreatePlatformView()
    {
        return new PopupPageRenderer(Context);
    }


    protected override void DisconnectHandler(ContentViewGroup platformView)
    {
        base.DisconnectHandler(platformView);
    }
}

