using System.Windows.Controls;
using System.Windows;
using SUI.Navigation;

namespace SuiExamples;

public partial class CustomNavigation : UserControl
{
    public CustomNavigation()
    {
        InitializeComponent();
        _wideItems = CreateItems();
        WideNav.ItemsSource = _wideItems.Where(i => i.Key is not ("settings" or "help")).ToList();
        WideNav.FooterItemsSource = _wideItems.Where(i => i.Key is "settings" or "help").ToList();
        WideNav.SelectedItem = _wideItems[0];
    }
    private List<SUINavigationItem> _wideItems = [];
    private int _changes;
    private static List<SUINavigationItem> CreateItems() =>
    [
        new() { Key = "overview", Title = "总览", IconGlyph = "\uE80F", ToolTip = "查看项目总览" },
        new() { Key = "projects", Title = "项目", IconGlyph = "\uE8B7", ToolTip = "管理当前项目" },
        new() { Key = "tools", Title = "工具", IconGlyph = "\uE90F", ToolTip = "查看常用工具" },
        new() { Key = "help", Title = "帮助", IconGlyph = "\uE897", ToolTip = "固定在底部的帮助入口" },
        new() { Key = "settings", Title = "设置", IconGlyph = "\uE713", IsEnabled = false, ToolTip = "暂不可用，可在上方启用" },
    ];
    private static string Description(string key) => key switch
    {
        "overview" => "在这里了解工作空间的整体状态。",
        "projects" => "在这里组织项目、资源和协作内容。",
        "tools" => "在这里放置高频使用的工具与操作。",
        "help" => "帮助与设置固定在侧栏底部，并复用横向图文模板。",
        _ => "在这里管理应用偏好与工作空间设置。",
    };
    private void OnWideSelection(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, WideNav) || e.AddedItems.Count != 1 || e.AddedItems[0] is not SUINavigationItem item) return;
        WideTitle.Text = item.Title; WideDescription.Text = Description(item.Key);
        SelectionLog.Text = $"当前页面：{item.Title}\n已切换 {++_changes} 次"; e.Handled = true;
    }
    private void OnOptionsChanged(object sender, RoutedEventArgs e)
    {
        if (_wideItems.Count == 0) return;
        var settings = _wideItems.First(i => i.Key == "settings");
        var tools = _wideItems.First(i => i.Key == "tools");
        var help = _wideItems.First(i => i.Key == "help");
        if (WideNav.SelectedItem == settings && EnableSettings.IsChecked != true
            || WideNav.SelectedItem == tools && ShowTools.IsChecked != true
            || WideNav.SelectedItem == help && ShowHelp.IsChecked != true)
            WideNav.SelectedItem = _wideItems[0];
        settings.IsEnabled = EnableSettings.IsChecked == true;
        settings.ToolTip = settings.IsEnabled ? "管理应用设置" : "暂不可用，可在上方启用";
        tools.IsVisible = ShowTools.IsChecked == true;
        help.IsVisible = ShowHelp.IsChecked == true;
    }
}
