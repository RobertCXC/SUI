using System.Windows.Controls;
using SUI.Navigation;

namespace SuiExamples;

public partial class CompactNavigation : UserControl
{
    public CompactNavigation()
    {
        InitializeComponent();
        var items = CreateItems();
        CompactNav.ItemsSource = items;
        CompactNav.FooterItemsSource = new List<SUINavigationItem>
        {
            new() { Key = "help", Title = "帮助", IconGlyph = "\uE897", ToolTip = "查看使用帮助" },
            new() { Key = "settings", Title = "设置", IconGlyph = "\uE713", ToolTip = "管理应用设置" },
        };
        CompactNav.SelectedItem = items[0];
    }
    private static List<SUINavigationItem> CreateItems() =>
    [
        new() { Key = "overview", Title = "总览", IconGlyph = "\uE80F", ToolTip = "查看项目总览" },
        new() { Key = "projects", Title = "项目", IconGlyph = "\uE8B7", ToolTip = "管理当前项目" },
        new() { Key = "tools", Title = "工具", IconGlyph = "\uE90F", ToolTip = "查看常用工具" },
        new() { Key = "reports", Title = "报告", IconGlyph = "\uE8A5", ToolTip = "查看项目报告" },
    ];
    private static string Description(string key) => key switch
    {
        "overview" => "在这里了解工作空间的整体状态。",
        "projects" => "在这里组织项目、资源和协作内容。",
        "tools" => "在这里放置高频使用的工具与操作。",
        "reports" => "在这里查看项目进展与汇总报告。",
        "help" => "帮助入口固定在侧栏底部，不随上方菜单滚动。",
        _ => "在这里管理应用偏好与工作空间设置。",
    };
    private void OnCompactSelection(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, CompactNav) || e.AddedItems.Count != 1 || e.AddedItems[0] is not SUINavigationItem item) return;
        CompactTitle.Text = item.Title; CompactDescription.Text = Description(item.Key); e.Handled = true;
    }
}
