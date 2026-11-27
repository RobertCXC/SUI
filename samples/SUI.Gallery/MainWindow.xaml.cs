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
        ("buttons", "按钮与命令", "\uE8A7", "Button ToggleButton Primary Success Ghost Danger Compact Icon 按钮 命令 紧凑 图标 成功", () => new ButtonsPage()),
        ("inputs", "输入与选择", "\uE70F", "TextBox PasswordBox TextBoxAssist PlaceholderText Compact SUINumberBox NumberBox ComboBox CheckBox RadioButton Pill ToggleButton Switch DatePicker Calendar TimePicker SUIDateTimePicker DateTimePicker FileUpload ListBox ScrollViewer 输入 占位提示 紧凑 数值 步进 密码 下拉 复选 单选 药丸 筛选 开关 日期 时间 文件上传 拖放 列表 滚动 验证", () => new InputsPage()),
        ("data", "数据与布局", "\uE80A", "DataGrid SUI.DataGrid.Compact TreeView GridSplitter 数据 表格 紧凑 树 分隔 布局", () => new DataPage()),
        ("containers", "容器与反馈", "\uE737", "SUIBadge Badge ContentStringFormat SUIInfoBar InfoBar SUIToastHost Toast SUIDialog SUIDialogOptions Dialog MessageBox SUICard SUISettingsCard SettingsCard GroupBox Expander SUIStepper Stepper SUI.Border.Card ProgressBar SUI.ProgressBar.Medium TabControlAssist IsClosable IsHeaderScrollable SUI.Separator SUI.Text.SectionHeader EqualityConverter 徽标 状态 胶囊 提示条 横幅 轻提示 弹窗 对话框 交互卡片 卡片 分组容器 折叠 步骤条 向导 设置卡片 分隔线 可关闭标签 横向滚动 标签 菜单 提示 进度", () => new ContainersPage()),
        ("navigation", "应用导航", "\uE700", "SUINavigationView SUINavigationItem PaneLayout Horizontal SUI.ListBox.Navigation 应用 导航 横向 二级导航 菜单", () => new NavigationPage()),
        ("tokens", "设计基础", "\uE790", "Colors Brush Font InfoSubtle NeutralSubtle SuccessSubtle WarningSubtle DangerSubtle 主题 颜色 字体 令牌 圆角 间距 设计", () => new TokensPage()),
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
            item.IsVisible = $"{entry.Title} {entry.Keywords}".IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
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
