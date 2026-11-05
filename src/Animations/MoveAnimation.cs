using Nkraft.Mopups.Enums;
using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Animations;

public class MoveAnimation : FadeBackgroundAnimation
{
    private const uint MoveDuration = 300;

    private double _defaultPageOpacity = 1;
    private double _defaultTranslationX;
    private double _defaultTranslationY;

    public MoveAnimationOptions PositionIn { get; set; }

    public MoveAnimationOptions PositionOut { get; set; }

    public MoveAnimation() : this(MoveAnimationOptions.Bottom, MoveAnimationOptions.Bottom)
    {
    }

    public MoveAnimation(MoveAnimationOptions positionIn, MoveAnimationOptions positionOut)
    {
        PositionIn = positionIn;
        PositionOut = positionOut;
        DurationIn = DurationOut = MoveDuration;
        EasingIn = Easing.SinOut;
        EasingOut = Easing.SinIn;
    }

    public override void Preparing(View? content, PopupPage page)
    {
        base.Preparing(content, page);

        _defaultPageOpacity = page.Opacity;
        HidePage(page);

        if (content is not null)
        {
            RememberTranslation(content);
        }
    }

    public override void Disposing(View? content, PopupPage page)
    {
        base.Disposing(content, page);

        page.Opacity = _defaultPageOpacity;

        if (content is not null)
        {
            content.TranslationX = _defaultTranslationX;
            content.TranslationY = _defaultTranslationY;
        }
    }

    public override Task AppearingAsync(View? content, PopupPage page)
    {
        List<Task> animations = [base.AppearingAsync(content, page)];

        if (content is not null)
        {
            // The content keeps its requested size. Pinning it to its measured size worked around a freeze on .NET 10
            // iOS (upstream pull request 177) that MAUI fixed in 10.0.50 (dotnet/maui issue 32586); kept, the pin
            // breaks layout when the content or the insets change (upstream pull request 183).
            var offset = Offset(PositionIn, content, page);
            content.TranslationX = _defaultTranslationX + offset.X;
            content.TranslationY = _defaultTranslationY + offset.Y;
            animations.Add(content.TranslateToAsync(_defaultTranslationX, _defaultTranslationY, DurationIn, EasingIn));
        }

        ShowPage(page);

        return Task.WhenAll(animations);
    }

    public override Task DisappearingAsync(View? content, PopupPage page)
    {
        List<Task> animations = [base.DisappearingAsync(content, page)];

        if (content is not null)
        {
            RememberTranslation(content);
            var offset = Offset(PositionOut, content, page);
            animations.Add(content.TranslateToAsync(
                _defaultTranslationX + offset.X, _defaultTranslationY + offset.Y, DurationOut, EasingOut));
        }

        return Task.WhenAll(animations);
    }

    private void RememberTranslation(View content)
    {
        _defaultTranslationX = content.TranslationX;
        _defaultTranslationY = content.TranslationY;
    }
}
