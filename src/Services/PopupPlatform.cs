using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Services;

internal sealed partial class PopupPlatform
{
    /// <summary>The app window the next popup opens over, or null while the app has none.</summary>
    public partial Window? HostWindow();

    /// <summary>
    /// Puts <paramref name="page"/> on screen, above <paramref name="host"/>'s content and above
    /// <paramref name="below"/>, the popup it covers. Returns once the page is laid out, so the animations that follow
    /// can read its size.
    /// </summary>
    public partial Task AddAsync(PopupPage page, Window host, PopupPage? below);

    /// <summary>Takes <paramref name="page"/> off screen. The page's handlers are still connected.</summary>
    public partial Task RemoveAsync(PopupPage page);
}
