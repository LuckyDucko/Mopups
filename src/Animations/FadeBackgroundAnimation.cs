using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Animations;

public abstract class FadeBackgroundAnimation : PopupAnimation
{
    private const string AnimationName = "backgroundFade";

    private Color? _backgroundColor;

    public bool HasBackgroundAnimation { get; set; } = true;

    public override void Preparing(View? content, PopupPage page)
    {
        _backgroundColor = page.BackgroundColor;
        if (HasBackgroundAnimation)
        {
            page.BackgroundColor = WithAlpha(0);
        }
    }

    public override void Disposing(View? content, PopupPage page)
    {
        if (HasBackgroundAnimation)
        {
            page.BackgroundColor = _backgroundColor;
        }
    }

    public override Task AppearingAsync(View? content, PopupPage page)
    {
        return HasBackgroundAnimation
            ? FadeBackgroundAsync(page, from: 0, to: _backgroundColor?.Alpha ?? 0, DurationIn, EasingIn)
            : Task.CompletedTask;
    }

    public override Task DisappearingAsync(View? content, PopupPage page)
    {
        if (!HasBackgroundAnimation)
            return Task.CompletedTask;

        _backgroundColor = page.BackgroundColor;
        return FadeBackgroundAsync(page, from: _backgroundColor?.Alpha ?? 0, to: 0, DurationOut, EasingOut);
    }

    private Task FadeBackgroundAsync(PopupPage page, double from, double to, uint length, Easing easing)
    {
        var finished = new TaskCompletionSource();
        page.Animate(
            AnimationName,
            alpha => page.BackgroundColor = WithAlpha(alpha),
            from,
            to,
            length: length,
            easing: easing,
            finished: (_, _) => finished.TrySetResult());
        return finished.Task;
    }

    private Color WithAlpha(double alpha) => (_backgroundColor ?? Colors.Transparent).WithAlpha((float)alpha);
}
