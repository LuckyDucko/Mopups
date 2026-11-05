using Nkraft.Mopups.Enums;
using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Animations;

public class ScaleAnimation : FadeAnimation
{
    private const double DefaultScale = 0.8;

    private const uint MoveDuration = 500;

    private double _defaultScale = 1;
    private double _defaultTranslationX;
    private double _defaultTranslationY;

    public double ScaleIn { get; set; } = DefaultScale;

    public double ScaleOut { get; set; } = DefaultScale;

    public MoveAnimationOptions PositionIn { get; set; }

    public MoveAnimationOptions PositionOut { get; set; }

    public ScaleAnimation() : this(MoveAnimationOptions.Center, MoveAnimationOptions.Center)
    {
    }

    public ScaleAnimation(MoveAnimationOptions positionIn, MoveAnimationOptions positionOut)
    {
        PositionIn = positionIn;
        PositionOut = positionOut;
        EasingIn = Easing.SinOut;
        EasingOut = Easing.SinIn;

        if (positionIn != MoveAnimationOptions.Center)
        {
            DurationIn = MoveDuration;
        }

        if (positionOut != MoveAnimationOptions.Center)
        {
            DurationOut = MoveDuration;
        }
    }

    public override void Preparing(View? content, PopupPage page)
    {
        base.Preparing(content, page);

        if (content is not null)
        {
            RememberTransform(content);
        }
    }

    public override void Disposing(View? content, PopupPage page)
    {
        base.Disposing(content, page);

        if (content is not null)
        {
            content.Scale = _defaultScale;
            content.TranslationX = _defaultTranslationX;
            content.TranslationY = _defaultTranslationY;
        }
    }

    public override Task AppearingAsync(View? content, PopupPage page)
    {
        List<Task> animations = [base.AppearingAsync(content, page)];

        if (content is not null)
        {
            content.Scale = _defaultScale * ScaleIn;
            animations.Add(content.ScaleToAsync(_defaultScale, DurationIn, EasingIn));

            var offset = Offset(PositionIn, content, page, ScaleIn);
            if (offset != Point.Zero)
            {
                content.TranslationX = _defaultTranslationX + offset.X;
                content.TranslationY = _defaultTranslationY + offset.Y;
                animations.Add(content.TranslateToAsync(
                    _defaultTranslationX, _defaultTranslationY, DurationIn, EasingIn));
            }
        }

        return Task.WhenAll(animations);
    }

    public override Task DisappearingAsync(View? content, PopupPage page)
    {
        List<Task> animations = [base.DisappearingAsync(content, page)];

        if (content is not null)
        {
            RememberTransform(content);
            animations.Add(content.ScaleToAsync(_defaultScale * ScaleOut, DurationOut, EasingOut));

            var offset = Offset(PositionOut, content, page, ScaleOut);
            if (offset != Point.Zero)
            {
                animations.Add(content.TranslateToAsync(
                    _defaultTranslationX + offset.X, _defaultTranslationY + offset.Y, DurationOut, EasingOut));
            }
        }

        return Task.WhenAll(animations);
    }

    private void RememberTransform(View content)
    {
        _defaultScale = content.Scale;
        _defaultTranslationX = content.TranslationX;
        _defaultTranslationY = content.TranslationY;
    }
}
