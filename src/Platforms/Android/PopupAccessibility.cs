using Android.Views;
using Nkraft.Mopups.Pages;
using AView = Android.Views.View;
using Window = Microsoft.Maui.Controls.Window;

namespace Nkraft.Mopups.Platforms.Android;

internal sealed class PopupAccessibility
{
    private readonly Dictionary<PopupPage, List<AView>> _coveredBy = [];
    private readonly Dictionary<AView, (ImportantForAccessibility Importance, DescendantFocusability? Focusability)>
        _originals = [];

    /// <summary>
    /// Hides what <paramref name="popup"/> covers from screen readers and keyboard focus while it is open;
    /// <see cref="Restore"/> puts it back once the last popup covering it closes (the fix for upstream issue 93, keyed
    /// by popup instance rather than type).
    /// </summary>
    public void Hide(PopupPage popup, Window host, PopupPage? below)
    {
        List<AView> covered = [];
        foreach (var view in CoveredViews(host, below))
        {
            if (!_originals.ContainsKey(view))
            {
                _originals[view] = (view.ImportantForAccessibility, (view as ViewGroup)?.DescendantFocusability);
                view.ImportantForAccessibility = ImportantForAccessibility.NoHideDescendants;
                if (view is ViewGroup group)
                {
                    group.DescendantFocusability = DescendantFocusability.BlockDescendants;
                }

                view.ClearFocus();
            }

            covered.Add(view);
        }

        _coveredBy[popup] = covered;
    }

    public void Restore(PopupPage popup)
    {
        if (!_coveredBy.Remove(popup, out var covered))
            return;

        foreach (var view in covered)
        {
            if (_coveredBy.Values.Any(views => views.Contains(view)))
                continue;

            if (_originals.Remove(view, out var original))
            {
                view.ImportantForAccessibility = original.Importance;
                if (view is ViewGroup group && original.Focusability is { } focusability)
                {
                    group.DescendantFocusability = focusability;
                }
            }
        }
    }

    /// <summary>
    /// The views under the popup: the window's page, the pages on top of its navigation and modal stacks, and the
    /// popup it covers.
    /// </summary>
    private static IEnumerable<AView> CoveredViews(Window host, PopupPage? below)
    {
        if (host.Page is not { } page)
            yield break;

        var pages = new List<Page?>
        {
            page,
            page.Navigation.NavigationStack.LastOrDefault(),
            page.Navigation.ModalStack.LastOrDefault(),
            below,
        };

        foreach (var view in pages.Select(p => p?.Handler?.PlatformView).OfType<AView>().Distinct())
        {
            yield return view;
        }
    }
}
