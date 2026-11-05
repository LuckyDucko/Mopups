using System.Diagnostics;
using Nkraft.CrossUtility.Extensions;
using Nkraft.Mopups.Animations;
using Nkraft.Mopups.Contracts;
using Nkraft.Mopups.Events;
using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Services;

internal sealed class PopupNavigation : IPopupNavigation
{
    private readonly PopupPlatform _platform = new();
    private readonly Lock _lock = new();
    private readonly Lock _queueLock = new();
    private readonly List<PopupPage> _stack = [];
    private readonly Dictionary<PopupPage, Task> _removals = [];
    private Task _operations = Task.CompletedTask;
    private volatile PopupPage[] _snapshot = [];

    public event EventHandler<PopupNavigationEventArgs>? Pushing;

    public event EventHandler<PopupNavigationEventArgs>? Pushed;

    public event EventHandler<PopupNavigationEventArgs>? Popping;

    public event EventHandler<PopupNavigationEventArgs>? Popped;

    public IReadOnlyList<PopupPage> PopupStack => _snapshot;

    /// <summary>
    /// Pushes, pops and removals run one at a time, on the main thread, in the order they were asked for, so a pop
    /// asked for right after a push waits for the push to finish instead of racing it. The stack changes as soon as an
    /// operation is asked for. Don't await another operation from inside a popup's animation hooks or
    /// <see cref="Pushing"/>/<see cref="Popping"/>: it would wait for the one running them.
    /// </summary>
    public Task PushAsync(PopupPage page, bool animate = true)
    {
        ArgumentNullException.ThrowIfNull(page);

        lock (_lock)
        {
            if (_stack.Contains(page))
                throw new InvalidOperationException("The page has already been pushed.");

            _stack.Add(page);
            _snapshot = [.. _stack];
        }

        return EnqueueAsync(() => PushCoreAsync(page, animate && page.IsAnimationEnabled));
    }

    public Task PopAsync(bool animate = true)
    {
        lock (_lock)
        {
            var top = _stack.LastOrDefault(page => !_removals.ContainsKey(page))
                ?? throw new InvalidOperationException("There is no popup to pop.");

            return RemoveAsync(top, animate);
        }
    }

    public Task PopAllAsync(bool animate = true)
    {
        lock (_lock)
        {
            var removals = _stack
                .Where(page => !_removals.ContainsKey(page))
                .Reverse()
                .Select(page => RemoveAsync(page, animate))
                .ToList();

            return Task.WhenAll(removals);
        }
    }

    public Task RemovePageAsync(PopupPage page, bool animate = true)
    {
        ArgumentNullException.ThrowIfNull(page);

        lock (_lock)
        {
            return RemoveAsync(page, animate);
        }
    }

    public bool SendBackButtonPressed()
    {
        PopupPage top;
        lock (_lock)
        {
            if (_stack.Count == 0)
                return false;

            top = _stack[^1];
            if (_removals.ContainsKey(top))
                return true;
        }

        if (!top.SendBackButtonPressed())
        {
            RemovePageAsync(top).FireAndForget(ex => Trace.TraceError("Closing a popup on Back failed: {0}", ex));
        }

        return true;
    }

    /// <summary>
    /// Under <see cref="_lock"/>: the check and the queueing are one step, so a page is queued for removal once.
    /// </summary>
    private Task RemoveAsync(PopupPage page, bool animate)
    {
        if (_removals.TryGetValue(page, out var removal))
            return removal;

        if (!_stack.Contains(page))
            return Task.CompletedTask;

        removal = EnqueueAsync(() => RemoveCoreAsync(page, animate && page.IsAnimationEnabled));
        _removals[page] = removal;
        return removal;
    }

    private Task EnqueueAsync(Func<Task> operation)
    {
        lock (_queueLock)
        {
            _operations = RunAfterAsync(_operations, operation);
            return _operations;
        }
    }

    private static async Task RunAfterAsync(Task previous, Func<Task> operation)
    {
        try
        {
            await previous.ConfigureAwait(false);
        }
        catch
        {
            // Its caller got the failure; the operations after it still run.
        }

        await MainThread.InvokeOnMainThreadAsync(operation);
    }

    private async Task PushCoreAsync(PopupPage page, bool animate)
    {
        animate = animate && SystemAnimations.Enabled;
        Pushing?.Invoke(this, new PopupNavigationEventArgs(page, animate));

        try
        {
            var host = _platform.HostWindow()
                ?? throw new InvalidOperationException("There is no window to show the popup over.");

            // Parent only, never a logical child of the host page: a NavigationPage would take the popup for its
            // current page and drop its bar (upstream issue 158).
            page.Parent = host.Page;
            page.PrepareAnimation(animate);
            await _platform.AddAsync(page, host, Below(page));
        }
        catch
        {
            page.Parent = null;
            Forget(page);
            throw;
        }

        page.SendAppearing();
        await page.AppearAsync();
        Pushed?.Invoke(this, new PopupNavigationEventArgs(page, animate));
    }

    private async Task RemoveCoreAsync(PopupPage page, bool animate)
    {
        animate = animate && SystemAnimations.Enabled;
        Popping?.Invoke(this, new PopupNavigationEventArgs(page, animate));

        try
        {
            await page.DisappearAsync(animate);
            page.SendDisappearing();
            await _platform.RemoveAsync(page);
            page.DisconnectHandlers();
            page.RestoreAnimation();
            page.Parent = null;
        }
        finally
        {
            Forget(page);
        }

        Popped?.Invoke(this, new PopupNavigationEventArgs(page, animate));
    }

    private PopupPage? Below(PopupPage page)
    {
        lock (_lock)
        {
            var index = _stack.IndexOf(page);
            return index > 0 ? _stack[index - 1] : null;
        }
    }

    private void Forget(PopupPage page)
    {
        lock (_lock)
        {
            _removals.Remove(page);
            _stack.Remove(page);
            _snapshot = [.. _stack];
        }
    }
}
