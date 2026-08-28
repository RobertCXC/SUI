using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace SUI.Navigation;

/// <summary>
/// 图标在上、标题在下的窄侧边导航壳。导航数据与右侧内容都由调用方提供。
/// </summary>
public class SUINavigationView : ContentControl
{
    static SUINavigationView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(SUINavigationView),
            new FrameworkPropertyMetadata(typeof(SUINavigationView)));
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(SUINavigationView),
            new PropertyMetadata(null));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(SUINavigationView),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged));

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(SUINavigationView),
            new PropertyMetadata(null));

    public Style? ItemContainerStyle
    {
        get => (Style?)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    public static readonly DependencyProperty ItemContainerStyleProperty =
        DependencyProperty.Register(
            nameof(ItemContainerStyle),
            typeof(Style),
            typeof(SUINavigationView),
            new PropertyMetadata(null));

    public GridLength PaneWidth
    {
        get => (GridLength)GetValue(PaneWidthProperty);
        set => SetValue(PaneWidthProperty, value);
    }

    public static readonly DependencyProperty PaneWidthProperty =
        DependencyProperty.Register(
            nameof(PaneWidth),
            typeof(GridLength),
            typeof(SUINavigationView),
            new PropertyMetadata(new GridLength(56d)));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(
            nameof(IconSize),
            typeof(double),
            typeof(SUINavigationView),
            new PropertyMetadata(18d));

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(
            nameof(ItemHeight),
            typeof(double),
            typeof(SUINavigationView),
            new PropertyMetadata(54d));

    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public static readonly DependencyProperty LabelFontSizeProperty =
        DependencyProperty.Register(
            nameof(LabelFontSize),
            typeof(double),
            typeof(SUINavigationView),
            new PropertyMetadata(11d));

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(SelectionChanged),
            RoutingStrategy.Bubble,
            typeof(SelectionChangedEventHandler),
            typeof(SUINavigationView));

    public event SelectionChangedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SUINavigationView)d;
        IList removedItems = e.OldValue is null ? Array.Empty<object>() : new[] { e.OldValue };
        IList addedItems = e.NewValue is null ? Array.Empty<object>() : new[] { e.NewValue };

        control.RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems));
    }
}
