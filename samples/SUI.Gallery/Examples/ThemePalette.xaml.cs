using System.Windows.Controls;
using System.Windows.Media;

namespace SuiExamples;

public partial class ThemePalette : UserControl
{
    public ThemePalette()
    {
        InitializeComponent();
        (string Name, string Role)[] colors =
        [
            ("Accent", "主要操作"), ("Selected", "选中背景"), ("Focus", "键盘焦点"),
            ("Background", "应用底色"), ("Surface", "内容表面"), ("SurfaceAlt", "次级表面"),
            ("Text", "主要文字"), ("TextSecondary", "辅助文字"), ("TextDisabled", "禁用文字"),
            ("Border", "默认边框"), ("Hover", "悬停背景"), ("TextOnAccent", "强调色上文字"),
            ("Success", "成功状态"), ("Warning", "注意状态"), ("Danger", "危险操作"),
        ];
        Palette.ItemsSource = colors.Select(c =>
        {
            var key = $"SUI.Brush.{c.Name}";
            var brush = (SolidColorBrush)FindResource(key);
            return new { c.Role, Brush = brush, Hex = $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}" };
        }).ToList();
    }
}
