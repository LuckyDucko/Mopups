using Android.App;
using AndroidX.Activity;
using AndroidX.Lifecycle;
using Nkraft.Mopups.Contracts;
using Nkraft.Mopups.Events;

namespace Nkraft.Mopups.Platforms.Android;

internal static class PopupBackPress
{
    private static readonly List<Callback> Callbacks = [];
    private static IPopupNavigation? _navigation;

    /// <summary>
    /// Routes the Back button to the popups through a callback on the back-press dispatcher of every window a popup
    /// is shown in (see <see cref="Watch"/>), enabled while a popup is open. MAUI's own callback, and its
    /// <c>OnBackPressed</c> lifecycle event, aren't enough: MAUI gives Back to the page under the popup as well, and
    /// gives it to nobody when that page can't go back.
    /// </summary>
    public static void Register(IPopupNavigation navigation)
    {
        if (_navigation is not null)
            return;

        _navigation = navigation;
        navigation.Pushing += HandleStackChanged;
        navigation.Popped += HandleStackChanged;
    }

    /// <summary>
    /// Called when a popup is added to <paramref name="owner"/>'s window: the activity, or a modal page's dialog, which
    /// has a dispatcher of its own. Added after MAUI's callback, so it takes the press first while enabled. The
    /// callback holds the dispatcher weakly, since a dialog's dispatcher holds the dialog and the callback outlives
    /// it; the owner's <see cref="Lifecycle"/> only holds the owner weakly, so it is safe to keep, and says when the
    /// window is gone.
    /// </summary>
    public static void Watch(OnBackPressedDispatcher dispatcher, ILifecycleOwner owner)
    {
        if (_navigation is null || Callbacks.Any(callback => callback.Watches(owner)))
            return;

        var callback = new Callback(_navigation, dispatcher, owner.Lifecycle)
        {
            Enabled = _navigation.PopupStack.Count > 0,
        };
        Callbacks.Add(callback);
        dispatcher.AddCallback(owner, callback);
    }

    private static void HandleStackChanged(object? sender, PopupNavigationEventArgs e)
    {
        var enabled = _navigation?.PopupStack.Count > 0;
        Callbacks.RemoveAll(callback => callback.IsGone);
        foreach (var callback in Callbacks)
        {
            callback.Enabled = enabled;
        }
    }

    private sealed class Callback(IPopupNavigation navigation, OnBackPressedDispatcher dispatcher, Lifecycle lifecycle)
        : OnBackPressedCallback(false)
    {
        private readonly IPopupNavigation _navigation = navigation;
        private readonly WeakReference<OnBackPressedDispatcher> _dispatcher = new(dispatcher);
        private readonly Lifecycle _lifecycle = lifecycle;

        public bool IsGone => _lifecycle.CurrentState == Lifecycle.State.Destroyed;

        public bool Watches(ILifecycleOwner owner) => ReferenceEquals(_lifecycle, owner.Lifecycle);

        public override void HandleOnBackPressed()
        {
            if (_navigation.SendBackButtonPressed())
                return;

            // Nothing to close after all: hand the press to the callbacks under this one.
            Enabled = false;
            if (_dispatcher.TryGetTarget(out var dispatcher))
            {
                dispatcher.OnBackPressed();
                Enabled = _navigation.PopupStack.Count > 0;
            }
        }
    }
}
