using Mopups.Pages;

namespace Mopups.Interfaces;

public interface IPopupPlatform
{
    Task AddAsync(PopupPage page);

    Task RemoveAsync(PopupPage page);
}
