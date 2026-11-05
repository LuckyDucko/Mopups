using Microsoft.Maui.LifecycleEvents;
using Nkraft.Mopups.Pages;
using Nkraft.Mopups.Platforms.Android;
using Nkraft.Mopups.Services;

namespace Nkraft.Mopups.Hosting;

public static partial class AppHostBuilderExtensions
{
    private static partial void ConfigurePlatform(MauiAppBuilder builder, Action? backPressHandler)
    {
        builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<PopupPage, PopupPageHandler>());
        PopupBackPress.Register(MopupService.Instance);

        if (backPressHandler is not null)
        {
            builder.ConfigureLifecycleEvents(lifecycle => lifecycle.AddAndroid(android => android.OnBackPressed(_ =>
            {
                if (MopupService.Instance.PopupStack.Count == 0)
                {
                    backPressHandler();
                }

                return false;
            })));
        }
    }
}
