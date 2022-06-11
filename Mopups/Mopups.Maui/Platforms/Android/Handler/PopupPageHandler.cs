using Android.Content;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Mopups.Droid.Gestures;
using Mopups.Platforms.Android.Renderers;

using System.Drawing;

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

        (platformView as PopupContentViewGroup).PopupHandler = this;
        base.ConnectHandler(platformView);
    }

    protected override ContentViewGroup CreatePlatformView()
    {
        var item = new PopupContentViewGroup(Context);
        return item;
    }


    protected override void DisconnectHandler(ContentViewGroup platformView)
    {
        //platformView.Dispose();
        base.DisconnectHandler(platformView);

    }
}

