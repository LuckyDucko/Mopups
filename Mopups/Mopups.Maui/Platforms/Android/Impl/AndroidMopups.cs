using System;
using Android.Views;
using Android.Widget;
using AndroidX.Activity;
using AndroidX.Fragment.App;
using AsyncAwaitBestPractices;
using Microsoft.Maui.Platform;
using Mopups.Interfaces;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Mopups.Pages;
using System.Collections.Generic;
using Mopups.Services;
using Microsoft.Maui.ApplicationModel;

namespace Mopups.Droid.Implementation;

public class AndroidMopups : IPopupPlatform
{
    private static FrameLayout? DecoreView => GetTopFragmentDecorView();

    private static WeakReference<FrameLayout>? _cachedDecorView;

    public static bool SendBackPressed(Action? backPressedHandler = null)
    {
        var popupNavigationInstance = MopupService.Instance;

        if (popupNavigationInstance.PopupStack.Count > 0)
        {
            var lastPage = popupNavigationInstance.PopupStack[popupNavigationInstance.PopupStack.Count - 1];

            var isPreventClose = lastPage.SendBackButtonPressed();

            if (!isPreventClose)
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

        page.Parent = MauiApplication.Current.Application.Windows[0].Content as Element;
        page.Parent ??= MauiApplication.Current.Application.Windows[0].Content as Element;

        var handler = page.Handler ??= new PopupPageHandler(page.Parent.Handler.MauiContext);

        var androidNativeView = handler.PlatformView as Android.Views.View;

        var decorView = DecoreView;
        if (decorView != null)
        {
            _cachedDecorView = new WeakReference<FrameLayout>(decorView);
            decorView.AddView(androidNativeView);
        }

        return PostAsync(androidNativeView);
    }

    public Task RemoveAsync(PopupPage page)
    {
        var renderer = IPopupPlatform.GetOrCreateHandler<PopupPageHandler>(page);

        if (renderer != null)
        {
            HandleAccessibility(false, page.DisableAndroidAccessibilityHandling, page);

            FrameLayout? decorView = null;

            if (_cachedDecorView != null && _cachedDecorView.TryGetTarget(out var cached))
            {
                decorView = cached;
            }
            else
            {
                decorView = DecoreView;
            }

            decorView?.RemoveView(renderer.PlatformView as Android.Views.View);
            renderer.DisconnectHandler(); // ?? good to leave as-is for cleanup
            page.Parent = null;

            _cachedDecorView = null; // clean up the reference

            return PostAsync(decorView);
        }

        return Task.CompletedTask;
    }

    //! important keeps reference to pages that accessibility has applied to. This is so accessibility can be removed properly when popup is removed. #https://github.com/LuckyDucko/Mopups/issues/93
    readonly Dictionary<Type, List<Android.Views.View>> accessibilityStates = new();
    void HandleAccessibility(bool showPopup, bool disableAccessibilityHandling, PopupPage popup)
    {
        if (disableAccessibilityHandling)
        {
            return;
        }

        if (showPopup)
        {
            Page? mainPage = popup.Parent as Page ?? Application.Current?.MainPage;

            if (mainPage is null)
            {
                return;
            }

            List<Android.Views.View> views = [];

            var mainPageAndroidView = mainPage.Handler?.PlatformView as Android.Views.View;
            if (mainPageAndroidView is not null && mainPageAndroidView.ImportantForAccessibility != ImportantForAccessibility.NoHideDescendants)
            {
                views.Add(mainPageAndroidView);
            }

            int navCount = mainPage.Navigation.NavigationStack.Count;
            if (navCount > 0)
            {
                var androidView = mainPage.Navigation.NavigationStack[navCount - 1]?.Handler?.PlatformView as Android.Views.View;

                if (androidView is not null && androidView.ImportantForAccessibility != ImportantForAccessibility.NoHideDescendants)
                {
                    views.Add(androidView);
                }
            }

            int modalCount = mainPage.Navigation.ModalStack.Count;
            if (modalCount > 0)
            {
                var androidView = mainPage.Navigation.ModalStack[modalCount - 1]?.Handler?.PlatformView as Android.Views.View;
                if (androidView is not null && androidView.ImportantForAccessibility != ImportantForAccessibility.NoHideDescendants)
                {
                    views.Add(androidView);
                }
            }

            var popupCount = MopupService.Instance.PopupStack.Count;
            if (popupCount > 1)
            {
                var androidView = MopupService.Instance.PopupStack[popupCount - 2]?.Handler?.PlatformView as Android.Views.View;
                if (androidView is not null && androidView.ImportantForAccessibility != ImportantForAccessibility.NoHideDescendants)
                {
                    views.Add(androidView);
                }
            }

            accessibilityStates.Add(popup.GetType(), views);
        }

        if (accessibilityStates.ContainsKey(popup.GetType()))
        {
            foreach (var view in accessibilityStates[popup.GetType()])
            {
                ProcessView(showPopup, view);
            }

            if (!showPopup)
            {
                accessibilityStates.Remove(popup.GetType());
            }
        }

        static void ProcessView(bool showPopup, Android.Views.View? view)
        {
            if (view is null)
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
        if (nativeView == null)
        {
            return Task.FromResult(true);
        }

        var tcs = new TaskCompletionSource<bool>();

        nativeView.Post(() => tcs.SetResult(true));

        return tcs.Task;
    }

    static FrameLayout? GetTopFragmentDecorView()
    {
        if (Platform.CurrentActivity is not ComponentActivity componentActivity)
        {
            return null;
        }

        var fragments = componentActivity.GetFragmentManager()?.Fragments;

        if (fragments is null || !fragments.Any())
        {
            return Platform.CurrentActivity?.Window?.DecorView as FrameLayout; ;
        }

        var topFragment = fragments[^1];

        if (topFragment is DialogFragment dialogFragment)
        {
            return dialogFragment.Dialog?.Window?.DecorView as FrameLayout;
        }

        return topFragment.Activity?.Window?.DecorView as FrameLayout;
    }
}
