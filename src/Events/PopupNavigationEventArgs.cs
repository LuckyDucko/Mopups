using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Events;

public class PopupNavigationEventArgs(PopupPage page, bool isAnimated) : EventArgs
{
    public PopupPage Page { get; } = page;

    public bool IsAnimated { get; } = isAnimated;
}
