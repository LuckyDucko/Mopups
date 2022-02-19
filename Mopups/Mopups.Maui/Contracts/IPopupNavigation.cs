using Mopups.Pages;

namespace Mopups.Interfaces;

public interface IPopupNavigation
{
    event EventHandler<PopupPage> Pushing;

    event EventHandler<PopupPage> Pushed;

    event EventHandler<PopupPage> Popping;

    event EventHandler<PopupPage> Popped;

    IReadOnlyList<PopupPage> PopupStack { get; }

    Task PushAsync(PopupPage page);

    Task PopAsync();

    Task PopAllAsync();

    Task RemovePageAsync(PopupPage page);
}
