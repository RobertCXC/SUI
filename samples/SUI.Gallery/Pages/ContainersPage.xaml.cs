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
}