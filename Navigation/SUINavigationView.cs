using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SUI.Navigation;

/// <summary>
/// 图标在上、标题在下的窄侧边导航壳。导航数据与右侧内容都由调用方提供。
/// </summary>
[TemplatePart(Name = NavigationListPartName, Type = typeof(ListBox))]
[TemplatePart(Name = FooterNavigationListPartName, Type = typeof(ListBox))]
public class SUINavigationView : ContentControl
{
    private const string NavigationListPartName = "PART_NavigationList";
    private const string FooterNavigationListPartName = "PART_FooterNavigationList";
    private ListBox? _navigationList;
    private ListBox? _footerNavigationList;
    private bool _isSynchronizingSelection;

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

    /// <summary>
    /// 固定显示在侧栏底部的导航项目，与 ItemsSource 共用 SelectedItem。
    /// </summary>
    public IEnumerable? FooterItemsSource
    {
        get => (IEnumerable?)GetValue(FooterItemsSourceProperty);
        set => SetValue(FooterItemsSourceProperty, value);
    }

    public static readonly DependencyProperty FooterItemsSourceProperty =
        DependencyProperty.Register(
            nameof(FooterItemsSource),
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

    public override void OnApplyTemplate()
    {
        if (_navigationList is not null)
        {
            _navigationList.SelectionChanged -= OnNavigationListSelectionChanged;
            ((INotifyCollectionChanged)_navigationList.Items).CollectionChanged -= OnNavigationItemsChanged;
        }

        if (_footerNavigationList is not null)
        {
            _footerNavigationList.SelectionChanged -= OnNavigationListSelectionChanged;
            ((INotifyCollectionChanged)_footerNavigationList.Items).CollectionChanged -= OnNavigationItemsChanged;
        }

        base.OnApplyTemplate();
        _navigationList = GetTemplateChild(NavigationListPartName) as ListBox;
        _footerNavigationList = GetTemplateChild(FooterNavigationListPartName) as ListBox;

        if (_navigationList is not null)
        {
            _navigationList.SelectionChanged += OnNavigationListSelectionChanged;
            ((INotifyCollectionChanged)_navigationList.Items).CollectionChanged += OnNavigationItemsChanged;
        }

        if (_footerNavigationList is not null)
        {
            _footerNavigationList.SelectionChanged += OnNavigationListSelectionChanged;
            ((INotifyCollectionChanged)_footerNavigationList.Items).CollectionChanged += OnNavigationItemsChanged;
        }

        SynchronizeSelection();
    }

    private void OnNavigationListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, sender))
        {
            return;
        }

        // 对外只冒泡控件自身的事件，切换分组时不会先发一次空选中事件。
        e.Handled = true;
        var list = (ListBox)sender;
        // WPF 销毁旧模板时也会清空列表；此时不能把临时状态写回公共选中项。
        if (_isSynchronizingSelection || !ReferenceEquals(list.TemplatedParent, this))
        {
            return;
        }

        if (list.SelectedItem is not null)
        {
            SetCurrentValue(SelectedItemProperty, list.SelectedItem);
        }
        else if (SelectedItem is not null && e.RemovedItems.Contains(SelectedItem))
        {
            SetCurrentValue(SelectedItemProperty, null);
        }
    }

    private void OnNavigationItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        bool isCurrentList = ReferenceEquals(sender, _navigationList?.Items) && ReferenceEquals(_navigationList?.TemplatedParent, this)
            || ReferenceEquals(sender, _footerNavigationList?.Items) && ReferenceEquals(_footerNavigationList?.TemplatedParent, this);
        if (!_isSynchronizingSelection && isCurrentList)
        {
            // 支持先绑定选中项、随后才填充或替换集合的 MVVM 加载顺序。
            SynchronizeSelection();
        }
    }

    private void SynchronizeSelection()
    {
        _isSynchronizingSelection = true;
        try
        {
            object? mainSelection = SelectedItem is not null && _navigationList?.Items.Contains(SelectedItem) == true
                ? SelectedItem
                : null;
            object? footerSelection = mainSelection is null && SelectedItem is not null && _footerNavigationList?.Items.Contains(SelectedItem) == true
                ? SelectedItem
                : null;

            _navigationList?.SetCurrentValue(Selector.SelectedItemProperty, mainSelection);
            _footerNavigationList?.SetCurrentValue(Selector.SelectedItemProperty, footerSelection);
        }
        finally
        {
            _isSynchronizingSelection = false;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (SUINavigationView)d;
        control.SynchronizeSelection();
        IList removedItems = e.OldValue is null ? Array.Empty<object>() : new[] { e.OldValue };
        IList addedItems = e.NewValue is null ? Array.Empty<object>() : new[] { e.NewValue };

        control.RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems));
    }
}
