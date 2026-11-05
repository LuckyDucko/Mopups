using Microsoft.Extensions.DependencyInjection.Extensions;
using Nkraft.Mopups.Contracts;
using Nkraft.Mopups.Services;

namespace Nkraft.Mopups.Hosting;

public static partial class AppHostBuilderExtensions
{
    /// <summary>
    /// Registers <see cref="IPopupNavigation"/> (the <see cref="MopupService.Instance"/>), the popup page's handler and
    /// the Back button handling. <paramref name="backPressHandler"/> runs on Android when Back is pressed with no
    /// popup open, before MAUI's own handling.
    /// </summary>
    public static MauiAppBuilder ConfigureMopups(this MauiAppBuilder builder, Action? backPressHandler = null)
    {
        builder.Services.TryAddSingleton(_ => MopupService.Instance);
        ConfigurePlatform(builder, backPressHandler);
        return builder;
    }

    private static partial void ConfigurePlatform(MauiAppBuilder builder, Action? backPressHandler);
}
