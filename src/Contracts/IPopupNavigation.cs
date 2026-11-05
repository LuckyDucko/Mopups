using Nkraft.Mopups.Events;
using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Contracts;

public interface IPopupNavigation
{
    event EventHandler<PopupNavigationEventArgs> Pushing;

    event EventHandler<PopupNavigationEventArgs> Pushed;

    event EventHandler<PopupNavigationEventArgs> Popping;

    event EventHandler<PopupNavigationEventArgs> Popped;

    IReadOnlyList<PopupPage> PopupStack { get; }

    Task PushAsync(PopupPage page, bool animate = true);

    Task PopAsync(bool animate = true);

    Task PopAllAsync(bool animate = true);

    /// <summary>
    /// Removes <paramref name="page"/> wherever it is in the stack. For a page that is already on its way out, waits
    /// for that removal instead; for a page that isn't in the stack, does nothing.
    /// </summary>
    Task RemovePageAsync(PopupPage page, bool animate = true);

    /// <summary>
    /// Gives the Back button to the topmost popup, and closes it unless its <see cref="Page.OnBackButtonPressed"/>
    /// handles the press. Returns whether there was a popup to give it to; a popup on its way out swallows the press.
    /// </summary>
    bool SendBackButtonPressed();
}
