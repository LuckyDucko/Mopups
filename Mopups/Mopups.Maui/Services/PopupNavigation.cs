using AsyncAwaitBestPractices;
using Mopups.Interfaces;
using Mopups.Pages;

namespace Mopups.Services;

public class PopupNavigation : IPopupNavigation
{
    private readonly object _locker = new();

    public IReadOnlyList<PopupPage> PopupStack => _popupStack;
    private readonly List<PopupPage> _popupStack = new();

    public event EventHandler<PopupPage> Pushing;

    public event EventHandler<PopupPage> Pushed;

    public event EventHandler<PopupPage> Popping;

    public event EventHandler<PopupPage> Popped;


    private static readonly Lazy<IPopupPlatform> lazyImplementation = new(() => GeneratePopupPlatform(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    private readonly IPopupPlatform PopupPlatform = lazyImplementation.Value;

    private static IPopupPlatform GeneratePopupPlatform()
    {
        return PullPlatformImplementation();


        static IPopupPlatform PullPlatformImplementation()
        {
#if ANDROID
            return new Mopups.Droid.Implementation.AndroidMopups();
#endif

            throw new PlatformNotSupportedException();
        }
    }

    private void OnInitialized(object? sender, EventArgs e)
    {
        if (_popupStack.Count > 0)
        {
            PopAllAsync().SafeFireAndForget();
        }
    }

    public Task PushAsync(PopupPage page)
    {
        Pushing?.Invoke(this, page);
        _popupStack.Add(page);

        return MainThread.IsMainThread
            ? PushPage()
            : MainThread.InvokeOnMainThreadAsync(PushPage);

        async Task PushPage()
        {
            await PopupPlatform.AddAsync(page);
            Pushed?.Invoke(this, page);
        };
    }

    public async Task PopAllAsync()
    {
        while (MopupService.Instance.PopupStack.Count > 0)
        {
            await PopAsync();
        }
    }

    public Task PopAsync()
    {
        return _popupStack.Count <= 0
            ? throw new InvalidOperationException("PopupStack is empty")
            : RemovePageAsync(PopupStack[PopupStack.Count - 1]);
    }

    public Task RemovePageAsync(PopupPage page)
    {

        if (page == null)
            throw new InvalidOperationException("Page can not be null");

        if (!_popupStack.Contains(page))
            throw new InvalidOperationException("The page has not been pushed yet or has been removed already");

        return (MainThread.IsMainThread
            ? RemovePage()
            : MainThread.InvokeOnMainThreadAsync(RemovePage));


        async Task RemovePage()
        {
            lock (_locker)
            {
                if (!_popupStack.Contains(page))
                {
                    return;
                }
            }

            Popping?.Invoke(this, page);
            await PopupPlatform.RemoveAsync(page);

            _popupStack.Remove(page);
            Popped?.Invoke(this, page);
        }
    }
}
