using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation.Peers;

namespace SUI;

/// <summary>徽标语义颜色。</summary>
public enum BadgeVariant { Neutral, Success, Warning, Danger, Info }
/// <summary>徽标外形。</summary>
public enum BadgeShape { Rounded, Pill }

/// <summary>用于连接状态、任务结果等简短信息的非交互徽标。</summary>
public class SUIBadge : ContentControl
{
    static SUIBadge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SUIBadge), new FrameworkPropertyMetadata(typeof(SUIBadge)));
        FocusableProperty.OverrideMetadata(typeof(SUIBadge), new FrameworkPropertyMetadata(false));
        IsTabStopProperty.OverrideMetadata(typeof(SUIBadge), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(BadgeVariant), typeof(SUIBadge),
        new FrameworkPropertyMetadata(BadgeVariant.Neutral),
        value => Enum.IsDefined(typeof(BadgeVariant), value));

    public BadgeVariant Variant
    {
        get => (BadgeVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public static readonly DependencyProperty ShapeProperty = DependencyProperty.Register(
        nameof(Shape), typeof(BadgeShape), typeof(SUIBadge),
        new FrameworkPropertyMetadata(BadgeShape.Pill),
        value => Enum.IsDefined(typeof(BadgeShape), value));

    public BadgeShape Shape
    {
        get => (BadgeShape)GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new BadgeAutomationPeer(this);

    private sealed class BadgeAutomationPeer : FrameworkElementAutomationPeer
    {
        public BadgeAutomationPeer(SUIBadge owner) : base(owner) { }
        protected override string GetClassNameCore() => nameof(SUIBadge);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Text;
        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            return string.IsNullOrEmpty(name) ? (Owner as SUIBadge)?.Content as string ?? string.Empty : name;
        }
    }
}
