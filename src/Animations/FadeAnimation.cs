using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Animations;

public class FadeAnimation : PopupAnimation
{
    private double _defaultOpacity = 1;

    public bool HasBackgroundAnimation { get; set; } = true;

    public override void Preparing(View? content, PopupPage page)
    {
        if (Target(content, page) is { } target)
        {
            _defaultOpacity = target.Opacity;
            target.Opacity = 0;
        }
    }

    public override void Disposing(View? content, PopupPage page)
    {
        if (Target(content, page) is { } target)
        {
            target.Opacity = _defaultOpacity;
        }
    }

    public override Task AppearingAsync(View? content, PopupPage page)
    {
        return Target(content, page)?.FadeToAsync(1, DurationIn, EasingIn) ?? Task.CompletedTask;
    }

    public override Task DisappearingAsync(View? content, PopupPage page)
    {
        if (Target(content, page) is not { } target)
            return Task.CompletedTask;

        _defaultOpacity = target.Opacity;
        return target.FadeToAsync(0, DurationOut, EasingOut);
    }

    /// <summary>What fades: the whole page, scrim included, or just its content.</summary>
    private VisualElement? Target(View? content, PopupPage page) => HasBackgroundAnimation ? page : content;
}
