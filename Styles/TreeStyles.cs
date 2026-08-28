using System.Windows;
using System.Windows.Media;

namespace SUI;

/// <summary>
/// TreeStyles 为 TreeView 提供可配置的视觉附加属性。
/// 配合 Styles/TreeStyles.xaml 使用，不绑定任何业务节点类型。
/// </summary>
public static class TreeStyles
{
    public static readonly DependencyProperty ItemBackgroundBrushProperty =
        RegisterBrush("ItemBackgroundBrush", Brushes.Transparent);

    public static readonly DependencyProperty HoverItemBackgroundBrushProperty =
        RegisterBrush("HoverItemBackgroundBrush", CreateBrush("#F3F7FC"));

    public static readonly DependencyProperty SelectedItemBackgroundBrushProperty =
        RegisterBrush("SelectedItemBackgroundBrush", CreateBrush("#E8F2FF"));

    public static readonly DependencyProperty ItemForegroundBrushProperty =
        RegisterBrush("ItemForegroundBrush", CreateBrush("#253247"));

    public static readonly DependencyProperty SelectedItemForegroundBrushProperty =
        RegisterBrush("SelectedItemForegroundBrush", CreateBrush("#1E5DA8"));

    public static readonly DependencyProperty SelectionIndicatorBrushProperty =
        RegisterBrush("SelectionIndicatorBrush", CreateBrush("#3478D4"));

    public static readonly DependencyProperty FolderIconBrushProperty =
        RegisterBrush("FolderIconBrush", CreateBrush("#4D86D1"));

    public static readonly DependencyProperty LeafIconBrushProperty =
        RegisterBrush("LeafIconBrush", CreateBrush("#8A9AB0"));

    public static readonly DependencyProperty ChevronBrushProperty =
        RegisterBrush("ChevronBrush", CreateBrush("#8A98AA"));

    public static readonly DependencyProperty ExpandedChevronBrushProperty =
        RegisterBrush("ExpandedChevronBrush", CreateBrush("#3878C7"));

    public static readonly DependencyProperty HierarchyLineBrushProperty =
        RegisterBrush("HierarchyLineBrush", CreateBrush("#E2EAF4"));

    public static readonly DependencyProperty OIForegroundBrushProperty =
        RegisterBrush("OIForegroundBrush", CreateBrush("#64748B"));

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

    public static readonly DependencyProperty OIColumnWidthProperty =
        DependencyProperty.RegisterAttached(
            "OIColumnWidth",
            typeof(double),
            typeof(TreeStyles),
            new FrameworkPropertyMetadata(58d));

    public static readonly DependencyProperty OIFontSizeProperty =
        DependencyProperty.RegisterAttached(
            "OIFontSize",
            typeof(double),
            typeof(TreeStyles),
            new FrameworkPropertyMetadata(11d));

    public static readonly DependencyProperty ItemFontSizeProperty =
        DependencyProperty.RegisterAttached(
            "ItemFontSize",
            typeof(double),
            typeof(TreeStyles),
            new FrameworkPropertyMetadata(13d));

    public static Brush GetItemBackgroundBrush(DependencyObject element) => GetBrush(element, ItemBackgroundBrushProperty);
    public static void SetItemBackgroundBrush(DependencyObject element, Brush? value) => element.SetValue(ItemBackgroundBrushProperty, value);

    public static Brush GetHoverItemBackgroundBrush(DependencyObject element) => GetBrush(element, HoverItemBackgroundBrushProperty);
    public static void SetHoverItemBackgroundBrush(DependencyObject element, Brush? value) => element.SetValue(HoverItemBackgroundBrushProperty, value);

    public static Brush GetSelectedItemBackgroundBrush(DependencyObject element) => GetBrush(element, SelectedItemBackgroundBrushProperty);
    public static void SetSelectedItemBackgroundBrush(DependencyObject element, Brush? value) => element.SetValue(SelectedItemBackgroundBrushProperty, value);

    public static Brush GetItemForegroundBrush(DependencyObject element) => GetBrush(element, ItemForegroundBrushProperty);
    public static void SetItemForegroundBrush(DependencyObject element, Brush? value) => element.SetValue(ItemForegroundBrushProperty, value);

    public static Brush GetSelectedItemForegroundBrush(DependencyObject element) => GetBrush(element, SelectedItemForegroundBrushProperty);
    public static void SetSelectedItemForegroundBrush(DependencyObject element, Brush? value) => element.SetValue(SelectedItemForegroundBrushProperty, value);

    public static Brush GetSelectionIndicatorBrush(DependencyObject element) => GetBrush(element, SelectionIndicatorBrushProperty);
    public static void SetSelectionIndicatorBrush(DependencyObject element, Brush? value) => element.SetValue(SelectionIndicatorBrushProperty, value);

    public static Brush GetFolderIconBrush(DependencyObject element) => GetBrush(element, FolderIconBrushProperty);
    public static void SetFolderIconBrush(DependencyObject element, Brush? value) => element.SetValue(FolderIconBrushProperty, value);

    public static Brush GetLeafIconBrush(DependencyObject element) => GetBrush(element, LeafIconBrushProperty);
    public static void SetLeafIconBrush(DependencyObject element, Brush? value) => element.SetValue(LeafIconBrushProperty, value);

    public static Brush GetChevronBrush(DependencyObject element) => GetBrush(element, ChevronBrushProperty);
    public static void SetChevronBrush(DependencyObject element, Brush? value) => element.SetValue(ChevronBrushProperty, value);

    public static Brush GetExpandedChevronBrush(DependencyObject element) => GetBrush(element, ExpandedChevronBrushProperty);
    public static void SetExpandedChevronBrush(DependencyObject element, Brush? value) => element.SetValue(ExpandedChevronBrushProperty, value);

    public static Brush GetHierarchyLineBrush(DependencyObject element) => GetBrush(element, HierarchyLineBrushProperty);
    public static void SetHierarchyLineBrush(DependencyObject element, Brush? value) => element.SetValue(HierarchyLineBrushProperty, value);

    public static Brush GetOIForegroundBrush(DependencyObject element) => GetBrush(element, OIForegroundBrushProperty);
    public static void SetOIForegroundBrush(DependencyObject element, Brush? value) => element.SetValue(OIForegroundBrushProperty, value);

    public static double GetItemMinHeight(DependencyObject element) => (double)element.GetValue(ItemMinHeightProperty);
    public static void SetItemMinHeight(DependencyObject element, double value) => element.SetValue(ItemMinHeightProperty, value);

    public static CornerRadius GetItemCornerRadius(DependencyObject element) => (CornerRadius)element.GetValue(ItemCornerRadiusProperty);
    public static void SetItemCornerRadius(DependencyObject element, CornerRadius value) => element.SetValue(ItemCornerRadiusProperty, value);

    public static double GetIconSize(DependencyObject element) => (double)element.GetValue(IconSizeProperty);
    public static void SetIconSize(DependencyObject element, double value) => element.SetValue(IconSizeProperty, value);

    public static double GetOIColumnWidth(DependencyObject element) => (double)element.GetValue(OIColumnWidthProperty);
    public static void SetOIColumnWidth(DependencyObject element, double value) => element.SetValue(OIColumnWidthProperty, value);

    public static double GetOIFontSize(DependencyObject element) => (double)element.GetValue(OIFontSizeProperty);
    public static void SetOIFontSize(DependencyObject element, double value) => element.SetValue(OIFontSizeProperty, value);

    public static double GetItemFontSize(DependencyObject element) => (double)element.GetValue(ItemFontSizeProperty);
    public static void SetItemFontSize(DependencyObject element, double value) => element.SetValue(ItemFontSizeProperty, value);

    private static DependencyProperty RegisterBrush(string name, Brush defaultValue)
    {
        return DependencyProperty.RegisterAttached(
            name,
            typeof(Brush),
            typeof(TreeStyles),
            new FrameworkPropertyMetadata(defaultValue));
    }

    private static Brush GetBrush(DependencyObject element, DependencyProperty property)
    {
        return (Brush?)element.GetValue(property) ?? Brushes.Transparent;
    }

    private static SolidColorBrush CreateBrush(string color)
    {
        var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(color)!;
        brush.Freeze();
        return brush;
    }
}
