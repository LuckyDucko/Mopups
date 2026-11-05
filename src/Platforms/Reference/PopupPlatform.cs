using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Services;

internal sealed partial class PopupPlatform
{
    public partial Window? HostWindow() => throw new PlatformNotSupportedException();

    public partial Task AddAsync(PopupPage page, Window host, PopupPage? below) =>
        throw new PlatformNotSupportedException();

    public partial Task RemoveAsync(PopupPage page) => throw new PlatformNotSupportedException();
}
