using System.Windows;
using System.Windows.Controls;

namespace SUI.Gallery.Pages;

public partial class ContainersPage : UserControl
{
    private int _nextTabIndex = 4;

    public ContainersPage() => InitializeComponent();

    private void OnInteractiveCardClick(object sender, RoutedEventArgs e)
    {
        if (sender is SUICard card)
            ToastHost.Show($"已点击卡片「{card.Title}」。", InfoSeverity.Info, "SUICard 交互");
    }

    private void OnCardActionClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string name })
            ToastHost.Show($"已打开「{name}」的参数配置（未误触发卡片点击）。", InfoSeverity.Success, "ActionContent 插槽");
    }

    private void OnAddClosableTab(object sender, RoutedEventArgs e)
    {
        var idx = _nextTabIndex++;
        var tab = new TabItem
        {
            Header = $"工作文档 #{idx}",
            Content = new TextBlock
            {
                Margin = new Thickness(12),
                Text = $"这是动态创建的第 {idx} 个可关闭文档页。可使用右侧 ×、中键或 Ctrl+F4 关闭。",
                TextWrapping = TextWrapping.Wrap,
            },
        };
        ClosableTabs.Items.Add(tab);
        ClosableTabs.SelectedItem = tab;
    }

    private void OnResetClosableTabs(object sender, RoutedEventArgs e)
    {
        while (ClosableTabs.Items.Count > 1)
            ClosableTabs.Items.RemoveAt(ClosableTabs.Items.Count - 1);
        _nextTabIndex = 1;
        for (var i = 0; i < 3; i++) OnAddClosableTab(sender, e);
        ClosableTabs.SelectedIndex = 0;
    }

    private void OnReopenInfoBar(object sender, RoutedEventArgs e) => InteractiveInfoBar.IsOpen = true;

    private void OnShowToast(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string kind }) return;
        var (title, msg, severity) = kind switch
        {
            "Success" => ("保存成功", "当前配置已写入本地文件。", InfoSeverity.Success),
            "Warning" => ("环境覆盖", "PORT 已被环境变量覆盖为 8080。", InfoSeverity.Warning),
            "Error" => ("连接失败", "无法连接到远程设备，请检查端口。", InfoSeverity.Error),
            _ => ("提示", "检测到一条新的状态通知。", InfoSeverity.Info),
        };
        ToastHost.Show(msg, severity, title);
    }

    private void OnOpenConfirmDialog(object sender, RoutedEventArgs e)
    {
        var ok = SUIDialog.Confirm("确定将所有设置恢复为出厂默认值吗？", "恢复默认设置", "恢复默认", "取消", owner: Window.GetWindow(this));
        ToastHost.Show(ok ? "已确认恢复默认设置。" : "已取消操作。", ok ? InfoSeverity.Success : InfoSeverity.Info);
    }

    private void OnOpenDangerDialog(object sender, RoutedEventArgs e)
    {
        var ok = SUIDialog.Confirm("将永久删除 128 MB 历史日志且无法恢复，是否继续？", "清理历史日志", "确认清理", "取消", isDanger: true, owner: Window.GetWindow(this));
        ToastHost.Show(ok ? "已执行日志清理。" : "已取消清理。", ok ? InfoSeverity.Warning : InfoSeverity.Info);
    }
}
