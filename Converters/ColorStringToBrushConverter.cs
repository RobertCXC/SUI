using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SUI.Converters;

/// <summary>
/// Converts color strings such as #4C79FF to brushes.
/// </summary>
public class ColorStringToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush DefaultBrush =
        new((Color)ColorConverter.ConvertFromString("#4C79FF"));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrWhiteSpace(colorString))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(colorString);
                return new SolidColorBrush(color);
            }
            catch
            {
            }
        }

        return DefaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is SolidColorBrush brush
            ? brush.Color.ToString()
            : DefaultBrush.Color.ToString();
    }
}
