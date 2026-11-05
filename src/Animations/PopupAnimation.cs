using System.ComponentModel;
using Nkraft.Mopups.Enums;
using Nkraft.Mopups.Pages;

namespace Nkraft.Mopups.Animations;

public interface IPopupAnimation
{
    void Preparing(View? content, PopupPage page);

    void Disposing(View? content, PopupPage page);

    Task AppearingAsync(View? content, PopupPage page);

    Task DisappearingAsync(View? content, PopupPage page);
}

public abstract class PopupAnimation : IPopupAnimation
{
    private const uint DefaultDuration = 200;

    public uint DurationIn { get; set; } = DefaultDuration;

    public uint DurationOut { get; set; } = DefaultDuration;

    public Easing EasingIn { get; set; } = Easing.Linear;

    public Easing EasingOut { get; set; } = Easing.Linear;

    /// <summary>
    /// Sets the page and its content up before the page is shown; <see cref="Disposing"/> undoes it once the page is
    /// gone, so the page can be shown again.
    /// </summary>
    public abstract void Preparing(View? content, PopupPage page);

    public abstract void Disposing(View? content, PopupPage page);

    public abstract Task AppearingAsync(View? content, PopupPage page);

    public abstract Task DisappearingAsync(View? content, PopupPage page);

    /// <summary>
    /// How far the content starts above, or ends below, its resting place: the page is laid out by then.
    /// </summary>
    protected virtual int GetTopOffset(View content, Page page) => (int)(content.Height + page.Height) / 2;

    protected virtual int GetLeftOffset(View content, Page page) => (int)(content.Width + page.Width) / 2;

    /// <summary>
    /// The content's translation at <paramref name="position"/>, off the page, relative to its resting place.
    /// <see cref="MoveAnimationOptions.Center"/> is no offset.
    /// </summary>
    protected Point Offset(MoveAnimationOptions position, View content, Page page, double scale = 1) => position switch
    {
        MoveAnimationOptions.Center => Point.Zero,
        MoveAnimationOptions.Top => new Point(0, -GetTopOffset(content, page) * scale),
        MoveAnimationOptions.Bottom => new Point(0, GetTopOffset(content, page) * scale),
        MoveAnimationOptions.Left => new Point(-GetLeftOffset(content, page) * scale, 0),
        MoveAnimationOptions.Right => new Point(GetLeftOffset(content, page) * scale, 0),
        _ => throw new InvalidEnumArgumentException(nameof(position), (int)position, typeof(MoveAnimationOptions)),
    };

    /// <summary>
    /// Keeps the page out of sight until <see cref="ShowPage"/>, through its opacity: hiding it with
    /// <see cref="VisualElement.IsVisible"/> would skip its layout, which the offsets need.
    /// </summary>
    protected static void HidePage(Page page) => page.Opacity = 0;

    /// <summary>
    /// Shows the page after the layout pass that applies the content's starting translation, so the content is never
    /// seen at its resting place first.
    /// </summary>
    protected static void ShowPage(Page page) => page.Dispatcher.Dispatch(() => page.Opacity = 1);
}
