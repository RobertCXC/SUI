using System.Windows;
using System.Windows.Controls;

namespace SUI.Gallery.Pages;

public partial class ContainersPage : UserControl
{
    public ContainersPage() => InitializeComponent();

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