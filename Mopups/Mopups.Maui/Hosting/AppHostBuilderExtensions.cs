using Microsoft.Maui.LifecycleEvents;
using Mopups.Pages;

namespace Mopups.Hosting;

/// <summary>
/// Represents application host extension, that used to configure handlers defined in Mopups.
/// </summary>
public static class AppHostBuilderExtensions
{
    /// <summary>
    /// Configures the implemented handlers in Syncfusion.Maui.Core.
    /// </summary>
    public static MauiAppBuilder ConfigureMopups(this MauiAppBuilder builder)
    {
        builder
            .ConfigureLifecycleEvents(lifecycle =>
            {
#if ANDROID
                lifecycle.AddAndroid(d =>
                {
                    d.OnBackPressed(activity => Droid.Implementation.AndroidMopups.SendBackPressed(activity.OnBackPressed));
                });
#endif
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                handlers.AddHandler(typeof(PopupPage), typeof(PopupPageHandler));
#endif
            });
        return builder;
    }
}
