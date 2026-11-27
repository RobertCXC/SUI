using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace SUI;

/// <summary>用于设置页或配置面板的条目卡片：左侧为图标、标题与副标题，右侧放置操作控件。</summary>
public class SUISettingsCard : ContentControl
{
    static SUISettingsCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SUISettingsCard), new FrameworkPropertyMetadata(typeof(SUISettingsCard)));
        FocusableProperty.OverrideMetadata(typeof(SUISettingsCard), new FrameworkPropertyMetadata(false));
        IsTabStopProperty.OverrideMetadata(typeof(SUISettingsCard), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(SUISettingsCard), new FrameworkPropertyMetadata(string.Empty));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(SUISettingsCard), new FrameworkPropertyMetadata(null));

    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IconGlyphProperty = DependencyProperty.Register(
        nameof(IconGlyph), typeof(string), typeof(SUISettingsCard), new FrameworkPropertyMetadata(null));

    public string? IconGlyph
    {
        get => (string?)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new SettingsCardAutomationPeer(this);

    private sealed class SettingsCardAutomationPeer(SUISettingsCard owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override string GetClassNameCore() => nameof(SUISettingsCard);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;
        protected override string GetNameCore() =>
            base.GetNameCore() is { Length: > 0 } name ? name : (Owner as SUISettingsCard)?.Title ?? string.Empty;
    }
}
