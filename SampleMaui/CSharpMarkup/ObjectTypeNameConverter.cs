using System.Globalization;

namespace SampleMaui.CSharpMarkup;

public sealed class ObjectTypeNameConverter : IValueConverter
{
    public static readonly IValueConverter Instance = new ObjectTypeNameConverter();

#region Implementation of IValueConverter

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Invert the bool
        return value.GetType().Name;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //return value.ToString();
        throw new NotSupportedException($"{nameof(ObjectTypeNameConverter)}.{nameof(this.ConvertBack)}");
    }

#endregion
}