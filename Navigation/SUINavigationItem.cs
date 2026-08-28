using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SUI.Navigation;

/// <summary>
/// 描述侧边导航中的一个项目。页面创建仍由使用控件的应用负责，
/// 因而控件库不会依赖任何具体业务页面。
/// </summary>
public sealed class SUINavigationItem : INotifyPropertyChanged
{
    private string _key = string.Empty;
    private string _title = string.Empty;
    private string _iconGlyph = string.Empty;
    private string? _toolTip;
    private bool _isEnabled = true;
    private bool _isVisible = true;

    public string Key
    {
        get => _key;
        set => SetField(ref _key, value);
    }

    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    /// <summary>
    /// Segoe Fluent Icons 字体中的字符。需要其他图标形式时，可通过
    /// SUINavigationView.ItemTemplate 完全替换默认显示模板。
    /// </summary>
    public string IconGlyph
    {
        get => _iconGlyph;
        set => SetField(ref _iconGlyph, value);
    }

    public string? ToolTip
    {
        get => _toolTip;
        set => SetField(ref _toolTip, value);
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
