using Android.Views;
using Android.Widget;
using AndroidX.Activity;
using AndroidX.Fragment.App;
using AsyncAwaitBestPractices;
using Mopups.Extensions;
using Microsoft.Maui.Platform;
using Mopups.Interfaces;
using Mopups.Pages;
using Mopups.Services;
using Activity = Android.App.Activity;
using System.Runtime.CompilerServices;

namespace Mopups.Droid.Implementation;

public class AndroidMopups : IPopupPlatform
{
    private static IList<FrameLayout?> DecoreViews => GetAllFragmentDecorViews();
    private static FrameLayout? DecoreView => GetTopFragmentDecorView();

    // One lifecycle-callbacks instance per activity (see RehostPopups).
    private static readonly ConditionalWeakTable<Activity, PopupHostFragmentCallbacks> FragmentCallbacks = new();

    public static bool SendBackPressed(Action? backPressedHandler = null)
    {
        var popupNavigationInstance = MopupService.Instance;

        if(popupNavigationInstance.PopupStack.Count > 0)
        {
            var lastPage = popupNavigationInstance.PopupStack[popupNavigationInstance.PopupStack.Count - 1];

            var isPreventClose = lastPage.SendBackButtonPressed();

            if(!isPreventClose)
            {
                popupNavigationInstance.PopAsync().SafeFireAndForget();
            }

            return true;
        }

        backPressedHandler?.Invoke();

        return false;
    }

    public Task AddAsync(PopupPage page)
    {
        HandleAccessibility(true, page.DisableAndroidAccessibilityHandling, page);

        page.Parent = IPlatformApplication.Current.Application.Windows[0].Content as Element;
        //var mainPage = (Element)MauiApplication.Current.Application.Windows[0].Content;
        //mainPage.AddLogicalChild(page);

        var handler = page.Handler ??= new PopupPageHandler(page.Parent.FindMauiContext());

        var androidNativeView = handler.PlatformView as Android.Views.View;

        // MAUI modal pages are DialogFragments, i.e. they live in their own window. A popup has to follow the topmost window
        // (see RehostPopups), otherwise it is hidden under a modal, or it is destroyed together with the modal it was added to.
        EnsureFragmentCallbacks();
        DecoreView?.AddView(androidNativeView);

        return PostAsync(androidNativeView);
    }
    
    public Task RemoveAsync(PopupPage page)
    {
        var renderer = IPopupPlatform.GetOrCreateHandler<PopupPageHandler>(page);

        if (renderer != null)
        {
            HandleAccessibility(false, page.DisableAndroidAccessibilityHandling, page);

            var popupView = renderer.PlatformView as Android.Views.View;

            // The popup may sit in a window that is no longer listed among the fragments (e.g. a dismissed modal) - detach it from its real parent first.
            (popupView?.Parent as ViewGroup)?.RemoveView(popupView);

            foreach (var decoreView in DecoreViews)
            {
                decoreView?.RemoveView(popupView);
            }
            renderer.DisconnectHandler(); //?? no clue if works
            page.Parent?.RemoveLogicalChild(page);

            return PostAsync(DecoreView);
        }

        return Task.CompletedTask;
    }

    //! important keeps reference to pages that accessibility has applied to. This is so accessibility can be removed properly when popup is removed. #https://github.com/LuckyDucko/Mopups/issues/93
    readonly Dictionary<Type, List<Android.Views.View>> accessibilityStates = new();
    void HandleAccessibility(bool showPopup, bool disableAccessibilityHandling, PopupPage popup)
    {
        if(disableAccessibilityHandling)
        {
            return;
        }

        if(showPopup)
        {
            Page? mainPage = popup.Parent as Page ?? Application.Current?.MainPage;

            if(mainPage is null)
            {
                return;
            }

            List<Android.Views.View> views = [];

            var mainPageAndroidView = mainPage.Handler?.PlatformView as Android.Views.View;
            if(mainPageAndroidView is not null && mainPageAndroidView.ImportantForAccessibility != ImportantForAccessibility.NoHideDescendants)
            {
                views.Add(mainPageAndroidView);
            }

            int navCount = mainPage.Navigation.NavigationStack.Count;
            if(navCount > 0)
            {
                var androidView = mainPage.Navigation.NavigationStack[navCount - 1]?.Handler?.PlatformView as Android.Views.View;

                if(androidView is not null && androidView.ImportantForAccessibility != ImportantForAccessibility.NoHideDescendants)
                {
                    views.Add(androidView);
                }
            }

            int modalCount = mainPage.Navigation.ModalStack.Count;
            if(modalCount > 0)
            {
                var androidView = mainPage.Navigation.ModalStack[modalCount - 1]?.Handler?.PlatformView as Android.Views.View;
                if(androidView is not null && androidView.ImportantForAccessibility != ImportantForAccessibility.NoHideDescendants)
                {
                    views.Add(androidView);
                }
            }

            var popupCount = MopupService.Instance.PopupStack.Count;
            if(popupCount > 1)
            {
                var androidView = MopupService.Instance.PopupStack[popupCount - 2]?.Handler?.PlatformView as Android.Views.View;
                if(androidView is not null && androidView.ImportantForAccessibility != ImportantForAccessibility.NoHideDescendants)
                {
                    views.Add(androidView);
                }
            }
            
            accessibilityStates.Add(popup.GetType(), views);
        }

        if(accessibilityStates.ContainsKey(popup.GetType()))
        {
            foreach(var view in accessibilityStates[popup.GetType()])
            {
                ProcessView(showPopup, view);
            }

            if(!showPopup)
            {
                accessibilityStates.Remove(popup.GetType());
            }
        }

        static void ProcessView(bool showPopup, Android.Views.View? view)
        {
            if(view is null)
            {
                return;
            }

            // Screen reader
            view.ImportantForAccessibility = showPopup ? ImportantForAccessibility.NoHideDescendants : ImportantForAccessibility.Auto;

            // Keyboard navigation
            ((ViewGroup)view).DescendantFocusability = showPopup ? DescendantFocusability.BlockDescendants : DescendantFocusability.AfterDescendants;
            view.ClearFocus();
        }
    }

    static Task<bool> PostAsync(Android.Views.View? nativeView)
    {
        if(nativeView == null)
        {
            return Task.FromResult(true);
        }

        var tcs = new TaskCompletionSource<bool>();

        nativeView.Post(() => tcs.SetResult(true));

        return tcs.Task;
    }
    
    static FrameLayout? GetTopFragmentDecorView(Fragment? exclude = null)
    {
        if (Platform.CurrentActivity is not ComponentActivity componentActivity)
        {
            return null;
        }

        var fragments = componentActivity.GetFragmentManager()?.Fragments;

        // The topmost dialog (MAUI modal page) that is actually on screen. Dialogs that are hidden, dismissed or being
        // destroyed (`exclude`) do not count - their window is not visible (or is about to be gone).
        if (fragments is not null)
        {
            for (var i = fragments.Count - 1; i >= 0; i--)
            {
                var fragment = fragments[i];
                if (fragment is DialogFragment dialogFragment
                    && !ReferenceEquals(fragment, exclude)
                    && dialogFragment.Dialog is { IsShowing: true } dialog)
                {
                    return dialog.Window?.DecorView as FrameLayout;
                }
            }
        }

        return Platform.CurrentActivity?.Window?.DecorView as FrameLayout;
    }

    static IList<FrameLayout?> GetAllFragmentDecorViews()
    {
        IList<FrameLayout?> decoreViews = new List<FrameLayout?>();
        if (Platform.CurrentActivity is not ComponentActivity componentActivity)
        {
            return decoreViews;
        }

        var fragments = componentActivity.GetFragmentManager()?.Fragments;

        if (fragments is null || !fragments.Any())
        {
            decoreViews.Add(Platform.CurrentActivity?.Window?.DecorView as FrameLayout);
            return decoreViews;
        }

        foreach (var fragment in fragments)
        {
            if (fragment is DialogFragment dialogFragment)
            {
                decoreViews.Add(dialogFragment.Dialog?.Window?.DecorView as FrameLayout);
                continue;
            }

            decoreViews.Add(fragment.Activity?.Window?.DecorView as FrameLayout);
        }

        return decoreViews;
    }

    static void EnsureFragmentCallbacks()
    {
        var activity = Platform.CurrentActivity;

        if (activity is not ComponentActivity componentActivity || FragmentCallbacks.TryGetValue(activity, out _))
        {
            return;
        }

        var fragmentManager = componentActivity.GetFragmentManager();
        if (fragmentManager is null)
        {
            return;
        }

        var callbacks = new PopupHostFragmentCallbacks();
        fragmentManager.RegisterFragmentLifecycleCallbacks(callbacks, false);
        FragmentCallbacks.Add(activity, callbacks);
    }

    /// <summary>
    /// Moves all visible popups to the window that is currently on top. A MAUI modal page is a DialogFragment with its own window:
    /// without this a popup that is shown while a modal is opened stays hidden under it, and a popup that was added to a modal
    /// disappears (but stays in the PopupStack) when that modal is closed.
    /// </summary>
    /// <param name="closingFragment">Dialog that is being destroyed and must not be used as a target.</param>
    static void RehostPopups(Fragment? closingFragment)
    {
        var popupStack = MopupService.Instance.PopupStack;
        if (popupStack.Count == 0)
        {
            return;
        }

        var target = GetTopFragmentDecorView(closingFragment);
        if (target is null)
        {
            return;
        }

        var popupViews = new List<Android.Views.View>(popupStack.Count);
        foreach (var popup in popupStack)
        {
            // A popup that is just being pushed has no native view yet.
            if (popup.Handler?.PlatformView is Android.Views.View popupView && popupView.Parent is not null)
            {
                popupViews.Add(popupView);
            }
        }

        if (popupViews.All(view => ReferenceEquals(view.Parent, target)))
        {
            return;
        }

        foreach (var popupView in popupViews)
        {
            if (!ReferenceEquals(popupView.Parent, target))
            {
                (popupView.Parent as ViewGroup)?.RemoveView(popupView);
                target.AddView(popupView);
            }
            else
            {
                popupView.BringToFront(); // keep the order of the PopupStack
            }
        }

        // Keep the stacking order of the popups (the first one is the bottom one).
        foreach (var popupView in popupViews)
        {
            popupView.BringToFront();
        }
    }

    sealed class PopupHostFragmentCallbacks : FragmentManager.FragmentLifecycleCallbacks
    {
        // A modal page was shown -> popups go above it.
        public override void OnFragmentStarted(FragmentManager fm, Fragment f)
        {
            if (f is DialogFragment)
            {
                RehostPopups(null);
            }
        }

        // A modal page was closed -> popups go to the window below it.
        public override void OnFragmentViewDestroyed(FragmentManager fm, Fragment f)
        {
            if (f is DialogFragment)
            {
                RehostPopups(f);
            }
        }

        public override void OnFragmentDestroyed(FragmentManager fm, Fragment f)
        {
            if (f is DialogFragment)
            {
                RehostPopups(f);
            }
        }
    }
}
