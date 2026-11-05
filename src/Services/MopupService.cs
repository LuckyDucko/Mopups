using Nkraft.Mopups.Contracts;

namespace Nkraft.Mopups.Services;

public static class MopupService
{
    private static readonly Lazy<PopupNavigation> Navigation = new(
        () => new PopupNavigation(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static IPopupNavigation Instance => Navigation.Value;
}
