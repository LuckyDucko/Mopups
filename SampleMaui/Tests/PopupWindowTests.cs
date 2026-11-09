using Mopups.Pages;
using Mopups.Services;
#if ANDROID
using Mopups.Platforms.Android.Renderers;
using Microsoft.Maui.Platform;
#endif

namespace SampleMaui.Tests;

/// <summary>
/// On-device tests of the window a popup lives in when modal pages are opened/closed.
/// Android: a MAUI modal page is a DialogFragment with its own window, a popup has to follow the topmost window.
/// The expected window is derived independently of Mopups (root view of the topmost modal page, else the activity decor view).
/// </summary>
public sealed class PopupWindowTests
{
    public sealed record TestCase(string Name, Func<Task> Run);

    public IReadOnlyList<TestCase> Tests { get; }

    public PopupWindowTests()
    {
        Tests =
        [
            new("Popup on page without modal is in the activity window", PopupOnPage),
            new("Popup shown on a modal is in the modal window", PopupOnModal),
            new("Modal closed while popup is shown: popup moves to the window below", ModalClosedWhilePopupShown),
            new("Modal opened while popup is shown: popup stays above it", ModalOpenedWhilePopupShown),
            new("Second modal opened and closed while popup is shown", SecondModalOpenedAndClosed),
            new("Order of several popups is kept when they move", PopupOrderIsKept),
            new("Popup removed after its modal was closed leaves nothing behind", RemoveAfterModalClosed),
        ];
    }

    /// <summary>Runs all tests, cleaning up popups and modals after each one. Returns (name, error or null).</summary>
    public async Task<IReadOnlyList<(string Name, string? Error)>> RunAllAsync()
    {
        var results = new List<(string, string?)>();
        foreach (var test in Tests)
        {
            string? error = null;
            try
            {
                await test.Run();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }
            finally
            {
                await CleanUpAsync();
            }

            System.Diagnostics.Debug.WriteLine($"MOPUPTEST {(error is null ? "PASS" : "FAIL")} {test.Name}{(error is null ? "" : " :: " + error)}");
            results.Add((test.Name, error));
        }

        var failed = results.Count(r => r.Item2 is not null);
        System.Diagnostics.Debug.WriteLine($"MOPUPTEST DONE pass={results.Count - failed} fail={failed}");
        return results;
    }

#if ANDROID
    static INavigation Nav => Application.Current!.Windows[0].Page!.Navigation;

    static Label NewLabel(string text) => new() { Text = text, BackgroundColor = Colors.White, Padding = 40, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

    static PopupPage NewPopup(string text) => new() { Content = NewLabel(text) };

    // Mopups keeps the accessibility state per popup *type* (AndroidMopups.HandleAccessibility), so two popups shown at once must be of different types.
    sealed class OtherPopup : PopupPage
    {
        public OtherPopup(string text) => Content = NewLabel(text);
    }

    static ContentPage NewModal() => new() { BackgroundColor = Colors.White, Content = new Label { Text = "modal", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center } };

    static Task Settle() => Task.Delay(400); // fragment transactions and lifecycle callbacks are asynchronous

    static async Task PushPopup(PopupPage popup)
    {
        await MopupService.Instance.PushAsync(popup, false);
        await Settle();
    }

    static async Task PushModal(Page modal)
    {
        await Nav.PushModalAsync(modal, false);
        await Settle();
    }

    static async Task PopModal()
    {
        await Nav.PopModalAsync(false);
        await Settle();
    }

    /// <summary>The window root the topmost popup is expected in: root of the topmost modal page, else the activity window.</summary>
    static Android.Views.View ExpectedRoot() =>
        (Nav.ModalStack.LastOrDefault()?.Handler?.PlatformView as Android.Views.View)?.RootView
        ?? Platform.CurrentActivity!.Window!.DecorView;

    static Android.Views.View NativeView(PopupPage popup) =>
        popup.Handler?.PlatformView as Android.Views.View ?? throw new Exception("Popup has no native view");

    static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    static void AssertOnTop(PopupPage popup, string where)
    {
        var view = NativeView(popup);
        AssertTrue(view.IsAttachedToWindow, $"{where}: popup view is not attached to a window (hidden/destroyed)");

        var expected = ExpectedRoot();
        AssertTrue(view.RootView.Handle == expected.Handle, $"{where}: popup is not in the topmost window");
    }

    static async Task CleanUpAsync()
    {
        while (MopupService.Instance.PopupStack.Count > 0)
        {
            await MopupService.Instance.PopAsync(false);
        }

        while (Nav.ModalStack.Count > 0)
        {
            await Nav.PopModalAsync(false);
        }

        await Settle();
    }

    static async Task PopupOnPage()
    {
        var popup = NewPopup("page");
        await PushPopup(popup);
        AssertOnTop(popup, "page");
    }

    static async Task PopupOnModal()
    {
        await PushModal(NewModal());
        var popup = NewPopup("on modal");
        await PushPopup(popup);
        AssertOnTop(popup, "on modal");
    }

    static async Task ModalClosedWhilePopupShown()
    {
        await PushModal(NewModal());
        var popup = NewPopup("closing modal");
        await PushPopup(popup);
        AssertOnTop(popup, "before close");

        await PopModal();
        AssertTrue(MopupService.Instance.PopupStack.Contains(popup), "popup was lost from the PopupStack");
        AssertOnTop(popup, "after the modal was closed");
    }

    static async Task ModalOpenedWhilePopupShown()
    {
        var popup = NewPopup("under new modal");
        await PushPopup(popup);
        AssertOnTop(popup, "before modal");

        await PushModal(NewModal());
        AssertOnTop(popup, "after a modal was opened");

        await PopModal();
        AssertOnTop(popup, "after that modal was closed");
    }

    static async Task SecondModalOpenedAndClosed()
    {
        await PushModal(NewModal());
        var popup = NewPopup("two modals");
        await PushPopup(popup);

        await PushModal(NewModal());
        AssertOnTop(popup, "above the second modal");

        await PopModal();
        AssertOnTop(popup, "back above the first modal");
    }

    static async Task PopupOrderIsKept()
    {
        await PushModal(NewModal());
        var bottom = NewPopup("bottom");
        PopupPage top = new OtherPopup("top");
        await PushPopup(bottom);
        await PushPopup(top);

        await PushModal(NewModal());
        AssertOnTop(bottom, "bottom popup");
        AssertOnTop(top, "top popup");

        var parent = (Android.Views.ViewGroup)NativeView(top).Parent!;
        AssertTrue(parent.IndexOfChild(NativeView(bottom)) < parent.IndexOfChild(NativeView(top)), "the popup pushed later is not above the earlier one");
    }

    static async Task RemoveAfterModalClosed()
    {
        await PushModal(NewModal());
        var popup = NewPopup("removed later");
        await PushPopup(popup);
        await PopModal();

        await MopupService.Instance.PopAsync(false);
        await Settle();

        AssertTrue(MopupService.Instance.PopupStack.Count == 0, "PopupStack is not empty");

        var decor = (Android.Views.ViewGroup)Platform.CurrentActivity!.Window!.DecorView;
        for (var i = 0; i < decor.ChildCount; i++)
        {
            AssertTrue(decor.GetChildAt(i) is not PopupPageRenderer, "a popup view is left in the activity window");
        }
    }
#else
    static Task CleanUpAsync() => Task.CompletedTask;

    static Task Skipped() => throw new NotSupportedException("Window tests are implemented for Android only");

    static Task PopupOnPage() => Skipped();
    static Task PopupOnModal() => Skipped();
    static Task ModalClosedWhilePopupShown() => Skipped();
    static Task ModalOpenedWhilePopupShown() => Skipped();
    static Task SecondModalOpenedAndClosed() => Skipped();
    static Task PopupOrderIsKept() => Skipped();
    static Task RemoveAfterModalClosed() => Skipped();
#endif
}
