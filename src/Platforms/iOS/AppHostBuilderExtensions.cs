namespace Nkraft.Mopups.Hosting;

public static partial class AppHostBuilderExtensions
{
    // No handler of its own: MAUI's PageHandler gives the page the view controller PopupPlatform embeds, and
    // iOS has no Back button.
    private static partial void ConfigurePlatform(MauiAppBuilder builder, Action? backPressHandler)
    {
    }
}
