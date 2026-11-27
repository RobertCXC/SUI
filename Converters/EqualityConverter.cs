using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace SUI.Converters;

/// <summary>
/// 比较两个值是否相等：同时支持单值绑定（<see cref="IValueConverter"/> 配合 <c>ConverterParameter</c>）
/// 与双值比较（<see cref="IMultiValueConverter"/>）。
/// </summary>
public sealed class EqualityConverter : IValueConverter, IMultiValueConverter
{
    /// <summary>共享的默认转换器实例。</summary>
    public static readonly EqualityConverter Instance = new();

    /// <summary>比较绑定值与 <paramref name="parameter"/> 是否相等。</summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        AreEqual(value, parameter, culture);

    /// <summary>当绑定目标为 <c>true</c> 时，将 <paramref name="parameter"/> 转换回目标类型写回数据源。</summary>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not true) return Binding.DoNothing;
        if (parameter is null) return null;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (underlying.IsInstanceOfType(parameter)) return parameter;
        if (parameter is string text && TryConvertFromString(text, underlying, culture, out var converted))
            return converted;

        return parameter;
    }

    /// <summary>比较双值 <see cref="IMultiValueConverter"/> 输入的两个值是否相等。</summary>
    public object Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values is null || values.Length != 2) return false;
        return AreEqual(values[0], values[1], culture);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static bool AreEqual(object? left, object? right, CultureInfo culture)
    {
        if (Equals(left, right)) return true;
        if (left is null || right is null) return false;

        return (left, right) switch
        {
            (Enum e, string s) => string.Equals(e.ToString(), s, StringComparison.Ordinal),
            (string s, Enum e) => string.Equals(e.ToString(), s, StringComparison.Ordinal),
            (_, string s) => TryConvertFromString(s, left.GetType(), culture, out var parsed) && Equals(left, parsed),
            (string s, _) => TryConvertFromString(s, right.GetType(), culture, out var parsed) && Equals(right, parsed),
            _ => false,
        };
    }

    private static bool TryConvertFromString(string text, Type targetType, CultureInfo culture, out object? converted)
    {
        if (targetType == typeof(string))
        {
            converted = text;
            return true;
        }

        if (targetType.IsEnum)
        {
#if NET8_0_OR_GREATER
            return Enum.TryParse(targetType, text, ignoreCase: false, out converted);
#else
            if (Enum.IsDefined(targetType, text))
            {
                converted = Enum.Parse(targetType, text, ignoreCase: false);
                return true;
            }
            try
            {
                converted = Enum.Parse(targetType, text, ignoreCase: false);
                return true;
            }
            catch (ArgumentException)
            {
                converted = null;
                return false;
            }
#endif
        }

        var format = culture ?? CultureInfo.InvariantCulture;
        switch (Type.GetTypeCode(targetType))
        {
            case TypeCode.Boolean:
                if (bool.TryParse(text, out var b)) { converted = b; return true; }
                break;
            case TypeCode.Byte:
                if (byte.TryParse(text, NumberStyles.Integer, format, out var u8)) { converted = u8; return true; }
                break;
            case TypeCode.SByte:
                if (sbyte.TryParse(text, NumberStyles.Integer, format, out var i8)) { converted = i8; return true; }
                break;
            case TypeCode.Int16:
                if (short.TryParse(text, NumberStyles.Integer, format, out var i16)) { converted = i16; return true; }
                break;
            case TypeCode.UInt16:
                if (ushort.TryParse(text, NumberStyles.Integer, format, out var u16)) { converted = u16; return true; }
                break;
            case TypeCode.Int32:
                if (int.TryParse(text, NumberStyles.Integer, format, out var i32)) { converted = i32; return true; }
                break;
            case TypeCode.UInt32:
                if (uint.TryParse(text, NumberStyles.Integer, format, out var u32)) { converted = u32; return true; }
                break;
            case TypeCode.Int64:
                if (long.TryParse(text, NumberStyles.Integer, format, out var i64)) { converted = i64; return true; }
                break;
            case TypeCode.UInt64:
                if (ulong.TryParse(text, NumberStyles.Integer, format, out var u64)) { converted = u64; return true; }
                break;
            case TypeCode.Single:
                if (float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, format, out var f32)) { converted = f32; return true; }
                break;
            case TypeCode.Double:
                if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, format, out var f64)) { converted = f64; return true; }
                break;
            case TypeCode.Decimal:
                if (decimal.TryParse(text, NumberStyles.Number, format, out var dec)) { converted = dec; return true; }
                break;
            case TypeCode.Char:
                if (text.Length == 1) { converted = text[0]; return true; }
                break;
            case TypeCode.DateTime:
                if (DateTime.TryParse(text, format, DateTimeStyles.None, out var dt)) { converted = dt; return true; }
                break;
        }

        if (targetType == typeof(Guid) && Guid.TryParse(text, out var guid))
        {
            converted = guid;
            return true;
        }

        if (targetType == typeof(TimeSpan) && TimeSpan.TryParse(text, format, out var ts))
        {
            converted = ts;
            return true;
        }

        var typeConverter = TypeDescriptor.GetConverter(targetType);
        if (typeConverter.CanConvertFrom(typeof(string)))
        {
            try
            {
                converted = typeConverter.ConvertFromString(null, format, text);
                return converted is not null;
            }
            catch (Exception ex) when (ex is FormatException or NotSupportedException or ArgumentException)
            {
                // Ignore invalid string parameter and return false below.
            }
        }

        converted = null;
        return false;
    }
}
