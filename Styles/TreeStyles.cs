using System.Windows;

namespace SUI;

/// <summary>
/// TreeViewItem 的布局附加属性。
/// </summary>
public static class TreeStyles
{
    public static readonly DependencyProperty ItemMinHeightProperty =
        DependencyProperty.RegisterAttached(
            "ItemMinHeight",
            typeof(double),
            typeof(TreeStyles),
            new FrameworkPropertyMetadata(36d));

    public static readonly DependencyProperty ItemCornerRadiusProperty =
        DependencyProperty.RegisterAttached(
            "ItemCornerRadius",
            typeof(CornerRadius),
            typeof(TreeStyles),
            new FrameworkPropertyMetadata(new CornerRadius(8)));

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.RegisterAttached(
            "IconSize",
            typeof(double),
            typeof(TreeStyles),
            new FrameworkPropertyMetadata(17d));

    public static readonly DependencyProperty ItemFontSizeProperty =
        DependencyProperty.RegisterAttached(
            "ItemFontSize",
            typeof(double),
            typeof(TreeStyles),
            new FrameworkPropertyMetadata(13d));

    public static double GetItemMinHeight(DependencyObject element) => (double)element.GetValue(ItemMinHeightProperty);
    public static void SetItemMinHeight(DependencyObject element, double value) => element.SetValue(ItemMinHeightProperty, value);

    public static CornerRadius GetItemCornerRadius(DependencyObject element) => (CornerRadius)element.GetValue(ItemCornerRadiusProperty);
    public static void SetItemCornerRadius(DependencyObject element, CornerRadius value) => element.SetValue(ItemCornerRadiusProperty, value);

    public static double GetIconSize(DependencyObject element) => (double)element.GetValue(IconSizeProperty);
    public static void SetIconSize(DependencyObject element, double value) => element.SetValue(IconSizeProperty, value);

    public static double GetItemFontSize(DependencyObject element) => (double)element.GetValue(ItemFontSizeProperty);
    public static void SetItemFontSize(DependencyObject element, double value) => element.SetValue(ItemFontSizeProperty, value);
}
