using Android.Views;
using AndroidX.Activity;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;
using Microsoft.Maui.Platform;
using Nkraft.Mopups.Pages;
using Nkraft.Mopups.Platforms.Android;
using AView = Android.Views.View;
using Window = Microsoft.Maui.Controls.Window;

namespace Nkraft.Mopups.Services;

internal sealed partial class PopupPlatform
{
    private readonly PopupAccessibility _accessibility = new();

    public partial Window? HostWindow()
    {
        var activity = Platform.CurrentActivity;
        var windows = Application.Current?.Windows ?? [];
        return windows.FirstOrDefault(window => ReferenceEquals(window.Handler?.PlatformView, activity))
            ?? windows.FirstOrDefault();
    }

    public partial async Task AddAsync(PopupPage page, Window host, PopupPage? below)
    {
        var context = host.Handler?.MauiContext
            ?? throw new InvalidOperationException("The host window has no MauiContext.");
        var (decorView, dispatcher, owner) = TopWindow()
            ?? throw new InvalidOperationException("There is no window to add the popup to.");

        var view = page.ToHandler(context).PlatformView as AView
            ?? throw new InvalidOperationException("The popup page's handler has no platform view.");

        if (!page.DisableAndroidAccessibilityHandling)
        {
            _accessibility.Hide(page, host, below);
        }

        PopupBackPress.Watch(dispatcher, owner);
        decorView.AddView(view);
        await PostAsync(view);
    }

    public partial Task RemoveAsync(PopupPage page)
    {
        _accessibility.Restore(page);

        // From whichever window holds it, and without waiting for that window's next frame: a modal page's window
        // on its way out never has one (upstream pull requests 155 and 156).
        if (page.Handler?.PlatformView is AView view)
        {
            (view.Parent as ViewGroup)?.RemoveView(view);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// The window a popup goes in, with its back-press dispatcher: the topmost modal page's dialog, or the activity.
    /// A modal page is a dialog fragment, and a popup added to the activity's window would sit under it.
    /// </summary>
    private static (ViewGroup DecorView, OnBackPressedDispatcher Dispatcher, ILifecycleOwner Owner)? TopWindow()
    {
        if (Platform.CurrentActivity is not ComponentActivity activity)
            return null;

        var fragments = activity.GetFragmentManager();
        // A modal page pushed a moment ago may still be a pending transaction (upstream pull request 164).
        fragments?.ExecutePendingTransactions();

        var dialog = fragments?.Fragments
            .OfType<DialogFragment>()
            .LastOrDefault(fragment => fragment.IsAdded && !fragment.IsRemoving && fragment.Dialog?.IsShowing == true)
            ?.Dialog;

        if (dialog is ComponentDialog componentDialog && dialog.Window?.DecorView is ViewGroup dialogDecorView)
            return (dialogDecorView, componentDialog.OnBackPressedDispatcher, componentDialog);

        return activity.Window?.DecorView is ViewGroup decorView
            ? (decorView, activity.OnBackPressedDispatcher, activity)
            : null;
    }

    /// <summary>Completes after the view's next layout pass.</summary>
    private static Task PostAsync(AView view)
    {
        var posted = new TaskCompletionSource();
        if (!view.Post(() => posted.TrySetResult()))
        {
            posted.TrySetResult();
        }

        return posted.Task;
    }
}
