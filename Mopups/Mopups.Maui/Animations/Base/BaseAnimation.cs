using System.ComponentModel;
using System.Globalization;
using System.Reflection;

using Mopups.Pages;

namespace Mopups.Animations.Base;

public class EasingTypeConverter : TypeConverter
{
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value != null)
        {
            var fieldInfo = typeof(Easing).GetRuntimeFields()?.FirstOrDefault(fi =>
            {
                if (fi.IsStatic)
                    return fi.Name == value.ToString();

                return false;
            });
            if (fieldInfo != null)
            {
                var fieldValue = fieldInfo.GetValue(null);

                if (fieldValue != null)
                    return (Easing)fieldValue;
            }
        }

        throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Easing)}");
    }
}
public class UintTypeConverter : TypeConverter
{
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        try
        {
            return Convert.ToUInt32(value);
        }
        catch (Exception)
        {
            throw new InvalidOperationException($"Cannot convert {value} into {typeof(uint)}");
        }
    }
}
public interface IPopupAnimation
{
    void Preparing(View content, PopupPage page);
    void Disposing(View content, PopupPage page);
    Task Appearing(View content, PopupPage page);
    Task Disappearing(View content, PopupPage page);
}
public abstract class BaseAnimation : IPopupAnimation
{
    private const uint DefaultDuration = 200;

    [TypeConverter(typeof(UintTypeConverter))]
    public uint DurationIn { get; set; } = DefaultDuration;

    [TypeConverter(typeof(UintTypeConverter))]
    public uint DurationOut { get; set; } = DefaultDuration;

    [TypeConverter(typeof(EasingTypeConverter))]
    public Easing EasingIn { get; set; } = Easing.Linear;

    [TypeConverter(typeof(EasingTypeConverter))]
    public Easing EasingOut { get; set; } = Easing.Linear;

    public abstract void Preparing(View content, PopupPage page);

    public abstract void Disposing(View content, PopupPage page);

    public abstract Task Appearing(View content, PopupPage page);

    public abstract Task Disappearing(View content, PopupPage page);

    protected virtual int GetTopOffset(View content, Page page)
    {
        return (int)(content.Height + page.Height) / 2;
    }

    protected virtual int GetLeftOffset(View content, Page page)
    {
        return (int)(content.Width + page.Width) / 2;
    }

    /// <summary>
    /// Cannot use Page.IsVisible as this will remove the ability to use GetTopOffset/GetLeftOffset
    /// </summary>
    /// <param name="page"></param>
    protected virtual void HidePage(Page page)
    {
        page.Opacity = 0;
    }

    /// <summary>
    /// Cannot use Page.IsVisible as this will remove the ability to use GetTopOffset/GetLeftOffset
    /// </summary>
    /// <param name="page"></param>
    protected virtual void ShowPage(Page page)
    {
        page.Dispatcher.Dispatch(() => page.Opacity = 1 );
    }
}
