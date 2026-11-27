using System.Collections;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SUI;

/// <summary>标签页关闭请求参数。</summary>
public sealed class TabCloseRequestedEventArgs(RoutedEvent routedEvent, TabItem tabItem, object item)
    : RoutedEventArgs(routedEvent, tabItem)
{
    /// <summary>请求关闭的标签容器。</summary>
    public TabItem TabItem { get; } = tabItem;

    /// <summary>对应的数据项；直接在 XAML 中声明 TabItem 时与 <see cref="TabItem"/> 相同。</summary>
    public object Item { get; } = item;
}

/// <summary>为原生 TabControl / TabItem 提供可关闭标签与标题栏横向滚动溢出能力。</summary>
public static class TabControlAssist
{
    /// <summary>关闭当前标签页的路由命令；SUI 的 TabItem 模板关闭按钮使用它，标签头获得焦点时也可按 Ctrl+F4。</summary>
    public static readonly RoutedUICommand CloseTabCommand = new(
        "关闭标签页", nameof(CloseTabCommand), typeof(TabControlAssist));

    /// <summary>标签页关闭请求事件；在 TabItem 上引发并向上冒泡，设置 Handled 可阻止后续命令执行与默认移除。</summary>
    public static readonly RoutedEvent CloseRequestedEvent = EventManager.RegisterRoutedEvent(
        "CloseRequested", RoutingStrategy.Bubble, typeof(EventHandler<TabCloseRequestedEventArgs>), typeof(TabControlAssist));

    public static void AddCloseRequestedHandler(DependencyObject element, EventHandler<TabCloseRequestedEventArgs> handler) =>
        (element as UIElement)?.AddHandler(CloseRequestedEvent, handler);

    public static void RemoveCloseRequestedHandler(DependencyObject element, EventHandler<TabCloseRequestedEventArgs> handler) =>
        (element as UIElement)?.RemoveHandler(CloseRequestedEvent, handler);

    /// <summary>是否显示关闭按钮；可设置在 TabControl 上统一开启，也可在单个 TabItem 上覆盖。</summary>
    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.RegisterAttached(
        "IsClosable", typeof(bool), typeof(TabControlAssist), new FrameworkPropertyMetadata(false, OnIsClosableChanged));

    public static bool GetIsClosable(DependencyObject element) => (bool)element.GetValue(IsClosableProperty);
    public static void SetIsClosable(DependencyObject element, bool value) => element.SetValue(IsClosableProperty, value);

    /// <summary>关闭命令；可设置在 TabControl 或 TabItem 上，TabItem 上的值优先。</summary>
    public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.RegisterAttached(
        "CloseCommand", typeof(ICommand), typeof(TabControlAssist), new FrameworkPropertyMetadata(null));

    public static ICommand? GetCloseCommand(DependencyObject element) => (ICommand?)element.GetValue(CloseCommandProperty);
    public static void SetCloseCommand(DependencyObject element, ICommand? value) => element.SetValue(CloseCommandProperty, value);

    /// <summary>关闭命令参数；未设置时传入标签对应的数据项。</summary>
    public static readonly DependencyProperty CloseCommandParameterProperty = DependencyProperty.RegisterAttached(
        "CloseCommandParameter", typeof(object), typeof(TabControlAssist), new FrameworkPropertyMetadata(null));

    public static object? GetCloseCommandParameter(DependencyObject element) => element.GetValue(CloseCommandParameterProperty);
    public static void SetCloseCommandParameter(DependencyObject element, object? value) => element.SetValue(CloseCommandParameterProperty, value);

    /// <summary>标签标题是否单行横向滚动（显示左右滚动与全部标签下拉按钮）；默认 false 保持 WPF 换行行为。</summary>
    public static readonly DependencyProperty IsHeaderScrollableProperty = DependencyProperty.RegisterAttached(
        "IsHeaderScrollable", typeof(bool), typeof(TabControlAssist), new FrameworkPropertyMetadata(false));

    public static bool GetIsHeaderScrollable(DependencyObject element) => (bool)element.GetValue(IsHeaderScrollableProperty);
    public static void SetIsHeaderScrollable(DependencyObject element, bool value) => element.SetValue(IsHeaderScrollableProperty, value);

    private static readonly DependencyPropertyKey HasHeaderOverflowPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
        "HasHeaderOverflow", typeof(bool), typeof(TabControlAssist), new FrameworkPropertyMetadata(false));

    /// <summary>横向滚动模式下标签标题是否超出可见宽度（只读）。</summary>
    public static readonly DependencyProperty HasHeaderOverflowProperty = HasHeaderOverflowPropertyKey.DependencyProperty;

    public static bool GetHasHeaderOverflow(DependencyObject element) => (bool)element.GetValue(HasHeaderOverflowProperty);

    /// <summary>模板内部使用：标记标题栏 ScrollViewer。</summary>
    public static readonly DependencyProperty IsHeaderScrollHostProperty = DependencyProperty.RegisterAttached(
        "IsHeaderScrollHost", typeof(bool), typeof(TabControlAssist), new FrameworkPropertyMetadata(false, OnIsHeaderScrollHostChanged));

    public static bool GetIsHeaderScrollHost(DependencyObject element) => (bool)element.GetValue(IsHeaderScrollHostProperty);
    public static void SetIsHeaderScrollHost(DependencyObject element, bool value) => element.SetValue(IsHeaderScrollHostProperty, value);

    /// <summary>模板内部使用：标记“全部标签”下拉按钮。</summary>
    public static readonly DependencyProperty IsOverflowMenuButtonProperty = DependencyProperty.RegisterAttached(
        "IsOverflowMenuButton", typeof(bool), typeof(TabControlAssist), new FrameworkPropertyMetadata(false, OnIsOverflowMenuButtonChanged));

    public static bool GetIsOverflowMenuButton(DependencyObject element) => (bool)element.GetValue(IsOverflowMenuButtonProperty);
    public static void SetIsOverflowMenuButton(DependencyObject element, bool value) => element.SetValue(IsOverflowMenuButtonProperty, value);

    private static readonly DependencyProperty IsMiddlePressedProperty = DependencyProperty.RegisterAttached(
        "IsMiddlePressed", typeof(bool), typeof(TabControlAssist), new FrameworkPropertyMetadata(false));

    static TabControlAssist()
    {
        CommandManager.RegisterClassCommandBinding(typeof(TabItem),
            new CommandBinding(CloseTabCommand, OnCloseTabExecuted, OnCloseTabCanExecute));
        CommandManager.RegisterClassInputBinding(typeof(TabItem),
            new InputBinding(CloseTabCommand, new KeyGesture(Key.F4, ModifierKeys.Control)));
        EventManager.RegisterClassHandler(typeof(TabItem), UIElement.MouseDownEvent,
            new MouseButtonEventHandler(OnTabItemMouseDown));
        EventManager.RegisterClassHandler(typeof(TabItem), UIElement.MouseUpEvent,
            new MouseButtonEventHandler(OnTabItemMouseUp));
        EventManager.RegisterClassHandler(typeof(TabItem), UIElement.MouseLeaveEvent,
            new MouseEventHandler(OnTabItemMouseLeave));
    }

    /// <summary>按 SUI 规则关闭标签：先引发 CloseRequested，再执行 CloseCommand；两者都未处理时从 Items 或可写的 ItemsSource 中移除。</summary>
    /// <returns>是否已处理关闭请求。</returns>
    public static bool RequestClose(TabItem tabItem)
    {
        if (tabItem == null) throw new ArgumentNullException(nameof(tabItem));
        if (!tabItem.IsEnabled || !GetIsClosable(tabItem)) return false;

        var owner = ItemsControl.ItemsControlFromItemContainer(tabItem) as TabControl;
        var item = GetItem(owner, tabItem);
        var args = new TabCloseRequestedEventArgs(CloseRequestedEvent, tabItem, item);
        tabItem.RaiseEvent(args);
        if (args.Handled) return true;

        var command = ResolveCommand(tabItem, owner);
        if (command is not null)
        {
            var parameter = GetCloseCommandParameter(tabItem) ?? item;
            if (!CanExecute(command, parameter, tabItem)) return false;
            Execute(command, parameter, tabItem);
            return true;
        }

        if (owner is null) return false;
        if (owner.ItemsSource is null)
        {
            owner.Items.Remove(item);
            return true;
        }

        if (owner.ItemsSource is IList { IsReadOnly: false, IsFixedSize: false } list)
        {
            list.Remove(item);
            return true;
        }

        return false;
    }

    private static void OnIsClosableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        CommandManager.InvalidateRequerySuggested();

    private static object GetItem(TabControl? owner, TabItem tabItem)
    {
        var item = owner?.ItemContainerGenerator.ItemFromContainer(tabItem);
        return item == null || item == DependencyProperty.UnsetValue ? tabItem : item;
    }

    private static ICommand? ResolveCommand(TabItem tabItem, TabControl? owner)
    {
        var command = GetCloseCommand(tabItem) ?? (owner is null ? null : GetCloseCommand(owner));
        return ReferenceEquals(command, CloseTabCommand) ? null : command;
    }

    private static bool CanExecute(ICommand command, object? parameter, IInputElement target) =>
        command is RoutedCommand routed ? routed.CanExecute(parameter, target) : command.CanExecute(parameter);

    private static void Execute(ICommand command, object? parameter, IInputElement target)
    {
        if (command is RoutedCommand routed) routed.Execute(parameter, target);
        else command.Execute(parameter);
    }

    private static void OnCloseTabExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TabItem tabItem && RequestClose(tabItem)) e.Handled = true;
    }

    private static void OnCloseTabCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is not TabItem tabItem) return;
        var closable = tabItem.IsEnabled && GetIsClosable(tabItem);
        if (closable)
        {
            var owner = ItemsControl.ItemsControlFromItemContainer(tabItem) as TabControl;
            var command = ResolveCommand(tabItem, owner);
            if (command is not null)
                closable = CanExecute(command, GetCloseCommandParameter(tabItem) ?? GetItem(owner, tabItem), tabItem);
        }

        e.CanExecute = closable;
        e.Handled = true;
    }

    private static void OnTabItemMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || sender is not TabItem tabItem) return;
        var inHeader = tabItem.IsEnabled && GetIsClosable(tabItem) && IsWithinHeaderBounds(tabItem, e);
        tabItem.SetValue(IsMiddlePressedProperty, inHeader);
    }

    private static void OnTabItemMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is TabItem tabItem)
            tabItem.ClearValue(IsMiddlePressedProperty);
    }

    private static void OnTabItemMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle || sender is not TabItem tabItem) return;
        var wasPressed = (bool)tabItem.GetValue(IsMiddlePressedProperty);
        tabItem.ClearValue(IsMiddlePressedProperty);
        if (wasPressed && IsWithinHeaderBounds(tabItem, e) && RequestClose(tabItem))
            e.Handled = true;
    }

    private static bool IsWithinHeaderBounds(TabItem tabItem, MouseEventArgs e) =>
        tabItem.ActualWidth > 0 &&
        tabItem.ActualHeight > 0 &&
        new Rect(tabItem.RenderSize).Contains(e.GetPosition(tabItem));

    private static void OnIsHeaderScrollHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer host) return;

        host.ScrollChanged -= OnHostScrollChanged;
        host.PreviewMouseWheel -= OnHostPreviewMouseWheel;
        host.Loaded -= OnHostLoaded;
        host.Unloaded -= OnHostUnloaded;
        if (!(bool)e.NewValue) return;

        host.ScrollChanged += OnHostScrollChanged;
        host.PreviewMouseWheel += OnHostPreviewMouseWheel;
        host.Loaded += OnHostLoaded;
        host.Unloaded += OnHostUnloaded;
        if (host.IsLoaded) OnHostLoaded(host, new RoutedEventArgs());
    }

    private static void OnHostLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ScrollViewer { TemplatedParent: TabControl tabControl } host) return;
        tabControl.SelectionChanged -= OnTabControlSelectionChanged;
        tabControl.SelectionChanged += OnTabControlSelectionChanged;
        UpdateOverflow(host);
        BringSelectedIntoView(tabControl);
    }

    private static void OnHostUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is ScrollViewer { TemplatedParent: TabControl tabControl })
            tabControl.SelectionChanged -= OnTabControlSelectionChanged;
    }

    private static void OnHostScrollChanged(object sender, ScrollChangedEventArgs e) => UpdateOverflow((ScrollViewer)sender);

    private static void UpdateOverflow(ScrollViewer host)
    {
        if (host.TemplatedParent is not TabControl tabControl) return;
        var overflowButtonsWidth =
            (tabControl.Template?.FindName("OverflowButtons", tabControl) as FrameworkElement)?.ActualWidth ?? 0.0;
        var availableWidth = host.ViewportWidth + overflowButtonsWidth;
        tabControl.SetValue(HasHeaderOverflowPropertyKey, host.ExtentWidth > availableWidth + 0.5);
    }

    private static void OnHostPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var host = (ScrollViewer)sender;
        if (host.ScrollableWidth <= 0 || e.Delta == 0) return;
        host.ScrollToHorizontalOffset(host.HorizontalOffset - e.Delta / 2.0);
        e.Handled = true;
    }

    private static void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, sender)) BringSelectedIntoView((TabControl)sender);
    }

    private static void BringSelectedIntoView(TabControl tabControl) =>
        tabControl.Dispatcher.InvokeAsync(() =>
        {
            if (tabControl.SelectedItem is null) return;
            var container = tabControl.SelectedItem as TabItem
                ?? tabControl.ItemContainerGenerator.ContainerFromItem(tabControl.SelectedItem) as TabItem;
            container?.BringIntoView();
        }, DispatcherPriority.Loaded);

    private static void OnIsOverflowMenuButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ButtonBase button) return;
        button.Click -= OnOverflowButtonClick;
        if ((bool)e.NewValue) button.Click += OnOverflowButtonClick;
    }

    private static void OnOverflowButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (ButtonBase)sender;
        if (button.TemplatedParent is not TabControl tabControl) return;

        var menu = new ContextMenu { PlacementTarget = button, Placement = PlacementMode.Bottom };
        for (var i = 0; i < tabControl.Items.Count; i++)
        {
            if (tabControl.ItemContainerGenerator.ContainerFromIndex(i) is not TabItem tabItem) continue;
            if (tabItem.Visibility != Visibility.Visible) continue;

            var menuItem = new MenuItem
            {
                IsCheckable = false,
                IsChecked = tabItem.IsSelected,
                IsEnabled = tabItem.IsEnabled,
            };
            if (tabItem.Header is UIElement)
            {
                menuItem.Header = GetHeaderText(tabItem);
            }
            else
            {
                menuItem.Header = tabItem.Header;
                menuItem.HeaderTemplate = tabItem.HeaderTemplate;
                menuItem.HeaderTemplateSelector = tabItem.HeaderTemplateSelector;
                menuItem.HeaderStringFormat = tabItem.HeaderStringFormat;
            }

            var target = tabItem;
            menuItem.Click += (_, _) =>
            {
                target.IsSelected = true;
                target.Focus();
            };
            menu.Items.Add(menuItem);
        }

        menu.IsOpen = true;
        e.Handled = true;
    }

    private static string GetHeaderText(TabItem tabItem)
    {
        var name = AutomationProperties.GetName(tabItem);
        if (!string.IsNullOrEmpty(name)) return name;

        var parts = new List<string>();
        CollectText(tabItem.Header as DependencyObject, parts);
        return parts.Count > 0 ? string.Join(" ", parts) : tabItem.Header?.ToString() ?? string.Empty;
    }

    private static void CollectText(DependencyObject? element, List<string> parts)
    {
        switch (element)
        {
            case null:
                return;
            case AccessText accessText:
                AddCleanedText(accessText.Text.Replace("_", string.Empty), parts);
                return;
            case TextBlock textBlock:
                if (!IsIconFont(textBlock.FontFamily))
                    AddCleanedText(textBlock.Text, parts);
                return;
        }

        foreach (var child in LogicalTreeHelper.GetChildren(element))
        {
            if (child is DependencyObject dep) CollectText(dep, parts);
            else if (child is string text) AddCleanedText(text, parts);
        }
    }

    private static bool IsIconFont(FontFamily? fontFamily)
    {
        var source = fontFamily?.Source;
        return source is not null &&
               (source.IndexOf("Segoe Fluent Icons", StringComparison.OrdinalIgnoreCase) >= 0 ||
                source.IndexOf("Segoe MDL2 Assets", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static void AddCleanedText(string? text, List<string> parts)
    {
        var cleaned = StripPrivateUse(text);
        if (!string.IsNullOrWhiteSpace(cleaned))
            parts.Add(cleaned);
    }

    private static string StripPrivateUse(string? text)
    {
        if (text is null || string.IsNullOrWhiteSpace(text)) return string.Empty;
        var sb = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                var cp = char.ConvertToUtf32(c, text[i + 1]);
                if (cp < 0xF0000)
                {
                    sb.Append(c);
                    sb.Append(text[i + 1]);
                }
                i++;
                continue;
            }

            if (c is < '\uE000' or > '\uF8FF')
                sb.Append(c);
        }

        return sb.ToString().Trim();
    }
}
