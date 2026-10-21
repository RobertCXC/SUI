using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SUI.Converters;

/// <summary>胶囊圆角始终为实际高度的一半，避免超大圆角被 WPF 压成椭圆。</summary>
public sealed class BadgeCornerRadiusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => new CornerRadius(value is double height && !double.IsNaN(height) && !double.IsInfinity(height)
            ? Math.Max(0, height / 2) : 0);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
