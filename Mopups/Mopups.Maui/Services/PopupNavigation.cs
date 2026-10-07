using AsyncAwaitBestPractices;

using Mopups.Events;
using Mopups.Interfaces;
using Mopups.Pages;

namespace Mopups.Services;

public class PopupNavigation : IPopupNavigation
{
    private readonly object _locker = new();

    public IReadOnlyList<PopupPage> PopupStack => _popupStack;
    private readonly List<PopupPage> _popupStack = new();
    private readonly Dictionary<PopupPage, Task> _removals = new();

    public event EventHandler<PopupNavigationEventArgs>? Pushing;

    public event EventHandler<PopupNavigationEventArgs>? Pushed;

    public event EventHandler<PopupNavigationEventArgs>? Popping;

    public event EventHandler<PopupNavigationEventArgs>? Popped;

    private static readonly Lazy<IPopupPlatform> lazyImplementation = new(() => GeneratePopupPlatform(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    private readonly IPopupPlatform PopupPlatform = lazyImplementation.Value;

    private static IPopupPlatform GeneratePopupPlatform()
    {
        return PullPlatformImplementation();


        static IPopupPlatform PullPlatformImplementation()
        {
#if ANDROID
            return new Mopups.Droid.Implementation.AndroidMopups();
#elif IOS
            return new Mopups.iOS.Implementation.iOSMopups();
#elif MACCATALYST
            return new Mopups.MacCatalyst.Implementation.MacOSMopups();
#elif WINDOWS
            return new Mopups.Windows.Implementation.PopupPlatformWindows();
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



    public Task PushAsync(PopupPage page, bool animate = true)
    {
        animate = animate && Animations.AnimationHelper.SystemAnimationsEnabled;

        Pushing?.Invoke(this, new PopupNavigationEventArgs(page, animate));
        _popupStack.Add(page);

        return MainThread.IsMainThread
            ? PushPage()
            : MainThread.InvokeOnMainThreadAsync(PushPage);

        async Task PushPage()
        {
            page.PreparingAnimation();
            await PopupPlatform.AddAsync(page);

            //Hack to make the popup to render within safe area
            if (page.HasSystemPadding)
            {
                page.Padding = new Thickness(page.SystemPadding.Left, page.SystemPadding.Top, page.SystemPadding.Right, page.SystemPadding.Bottom);
            }

            page.SendAppearing();
            await page.AppearingAnimation();
            Pushed?.Invoke(this, new PopupNavigationEventArgs(page, animate));
        };
    }

    public async Task PopAllAsync(bool animate = true)
    {
		animate = animate && Animations.AnimationHelper.SystemAnimationsEnabled;

		while (MopupService.Instance.PopupStack.Count > 0)
        {
            await PopAsync(animate);
        }
    }

    public Task PopAsync(bool animate = true)
    {
		animate = animate && Animations.AnimationHelper.SystemAnimationsEnabled;

		return _popupStack.Count <= 0
            ? throw new InvalidOperationException("PopupStack is empty")
            : RemovePageAsync(PopupStack[PopupStack.Count - 1], animate);
    }

    public Task RemovePageAsync(PopupPage page, bool animate = true)
    {
		animate = animate && Animations.AnimationHelper.SystemAnimationsEnabled;

		if (page == null)
            throw new InvalidOperationException("Page can not be null");

        TaskCompletionSource removed;

        lock (_locker)
        {
            // The page stays in the stack until its removal finishes, so a call made meanwhile
            // (background tap, PopAsync, an awaiting caller) shares that removal instead of starting another.
            if (_removals.TryGetValue(page, out var inProgress))
                return inProgress;

            if (!_popupStack.Contains(page))
                throw new InvalidOperationException("The page has not been pushed yet or has been removed already");

            removed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _removals.Add(page, removed.Task);
        }

        _ = RunRemoval();
        return removed.Task;

        async Task RunRemoval()
        {
            Exception? failure = null;

            try
            {
                await (MainThread.IsMainThread
                    ? RemovePage()
                    : MainThread.InvokeOnMainThreadAsync(RemovePage));
            }
            catch (Exception ex)
            {
                failure = ex;
            }

            lock (_locker)
            {
                _removals.Remove(page);
            }

            if (failure == null)
                removed.SetResult();
            else
                removed.SetException(failure);
        }

        async Task RemovePage()
        {
            lock (_locker)
            {
                if (!_popupStack.Contains(page))
                {
                    return;
                }
            }

            Popping?.Invoke(this, new PopupNavigationEventArgs(page, animate));
            await page.DisappearingAnimation();
            page.SendDisappearing();
            await PopupPlatform.RemoveAsync(page);
            page.DisposingAnimation();

            _popupStack.Remove(page);
            Popped?.Invoke(this, new PopupNavigationEventArgs(page, animate));
        }
    }
}

