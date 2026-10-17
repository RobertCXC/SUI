using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SUI.Gallery.Pages;
using SUI.Navigation;

namespace SUI.Gallery;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, UserControl> _cache = new();
    private readonly (string Key, string Title, string Glyph, string Keywords, Func<UserControl> Create)[] _catalog =
    [
        ("home", "概览", "\uE80F", "首页 overview 开始", () => new HomePage()),
        ("buttons", "按钮与命令", "\uE8A7", "Button ToggleButton Primary Ghost Danger 按钮 命令", () => new ButtonsPage()),
        ("inputs", "输入与选择", "\uE70F", "TextBox PasswordBox ComboBox CheckBox RadioButton Switch ListBox ScrollViewer 输入 密码 下拉 复选 单选 开关 列表 滚动 验证", () => new InputsPage()),
        ("data", "数据与布局", "\uE80A", "DataGrid TreeView GridSplitter 数据 表格 树 分隔 布局", () => new DataPage()),
        ("containers", "容器与反馈", "\uE737", "SUICard TabControl Menu ContextMenu ToolTip ProgressBar 容器 卡片 标签 菜单 提示 进度", () => new ContainersPage()),
        ("navigation", "应用导航", "\uE700", "SUINavigationView SUINavigationItem 应用 导航", () => new NavigationPage()),
        ("tokens", "设计基础", "\uE790", "Colors Brush Font 主题 颜色 字体 令牌 圆角 间距 设计", () => new TokensPage()),
    ];
    private List<SUINavigationItem> _items = [];

    public MainWindow()
    {
        InitializeComponent();
        Width = Math.Min(Width, SystemParameters.WorkArea.Width);
        Height = Math.Min(Height, SystemParameters.WorkArea.Height);
        _items = _catalog.Select(p => new SUINavigationItem
        {
            Key = p.Key,
            Title = p.Title,
            IconGlyph = p.Glyph,
            ToolTip = p.Key switch
            {
                "home" => "探索组件与运行效果",
                "buttons" => "按钮变体、状态与交互试验台",
                "inputs" => "文本输入、校验与偏好设置",
                "data" => "表格、树与可调整工作区",
                "containers" => "卡片、标签、菜单与任务反馈",
                "navigation" => "侧栏布局、选中与入口状态",
                _ => "颜色、字体与布局令牌",
            },
        }).ToList();
        Navigation.ItemsSource = _items;
        Navigate("home");
    }

    private void Navigate(string key)
    {
        Search.Clear();
        Navigation.SelectedItem = _items.First(i => i.Key == key);
    }

    private void OnNavigationSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, Navigation)) return;
        if (e.AddedItems.Count != 1 || e.AddedItems[0] is not SUINavigationItem item) return;
        if (!_cache.TryGetValue(item.Key, out var page))
        {
            page = _catalog.First(p => p.Key == item.Key).Create();
            _cache.Add(item.Key, page);
        }
        PageHost.Content = page;
        Status.Text = $"{item.Title} · 示例状态会在切换页面后保留";
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        if (_items.Count == 0) return;
        var query = Search.Text.Trim();
        foreach (var item in _items)
        {
            var entry = _catalog.First(p => p.Key == item.Key);
            item.IsVisible = $"{entry.Title} {entry.Keywords}".Contains(query, StringComparison.OrdinalIgnoreCase);
        }
        SearchEmpty.Visibility = _items.Any(i => i.IsVisible) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.K && Keyboard.Modifiers == ModifierKeys.Control)
        {
            Search.Focus(); Search.SelectAll(); e.Handled = true;
        }
        else if (e.Key == Key.Escape && Search.IsKeyboardFocused)
        {
            Search.Clear(); e.Handled = true;
        }
        else if (e.Key == Key.Enter && Search.IsKeyboardFocused)
        {
            var item = _items.FirstOrDefault(i => i.IsVisible);
            if (item is not null)
            {
                Navigate(item.Key);
                PageHost.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
            e.Handled = true;
        }
    }

    private void OnPageAction(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Button { Tag: string tag } && _catalog.Any(p => p.Key == tag)) Navigate(tag);
    }
}
