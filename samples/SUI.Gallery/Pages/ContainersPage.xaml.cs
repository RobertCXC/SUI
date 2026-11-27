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

    private void OnOpenFormDialog(object sender, RoutedEventArgs e)
    {
        var nameInput = new TextBox { Width = 360 };
        TextBoxAssist.SetPlaceholderText(nameInput, "输入方案名称（不可为空或 default）");
        var descInput = new TextBox { Width = 360, Height = 72, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
        TextBoxAssist.SetPlaceholderText(descInput, "可选备注说明...");

        var panel = new StackPanel();
        panel.Children.Add(new TextBlock { Text = "方案名称", Margin = new Thickness(0, 0, 0, 6) });
        panel.Children.Add(nameInput);
        panel.Children.Add(new TextBlock { Text = "备注描述", Margin = new Thickness(0, 12, 0, 0) });
        panel.Children.Add(descInput);

        var result = SUIDialog.Show(panel, new SUIDialogOptions
        {
            Title = "新建升级方案（含校验与宽屏支持）",
            PrimaryText = "创建方案",
            CancelText = "取消",
            Owner = Window.GetWindow(this),
            MaxWidth = 600,
            Closing = (_, args) =>
            {
                if (args.Result != SUIDialogResult.Primary) return;
                var val = nameInput.Text.Trim();
                if (string.IsNullOrEmpty(val))
                {
                    args.Cancel = true;
                    args.ErrorMessage = "方案名称不能为空，请输入后重试。";
                }
                else if (string.Equals(val, "default", StringComparison.OrdinalIgnoreCase))
                {
                    args.Cancel = true;
                    args.ErrorMessage = "方案名称「default」已存在，请更换名称。";
                }
            },
        });

        if (result == SUIDialogResult.Primary)
        {
            ToastHost.Show($"已创建方案「{nameInput.Text.Trim()}」。", InfoSeverity.Success, "SUIDialog 表单");
        }
    }

    private void OnStepperPrev(object sender, RoutedEventArgs e)
    {
        UpgradeStepper.IsCurrentStepError = false;
        UpgradeStepper.CurrentStepIndex = Math.Max(0, UpgradeStepper.CurrentStepIndex - 1);
        UpdateStepperStatus();
    }

    private void OnStepperNext(object sender, RoutedEventArgs e)
    {
        UpgradeStepper.IsCurrentStepError = false;
        UpgradeStepper.CurrentStepIndex = Math.Min(UpgradeStepper.Items.Count, UpgradeStepper.CurrentStepIndex + 1);
        UpdateStepperStatus();
    }

    private void OnStepperToggleError(object sender, RoutedEventArgs e)
    {
        if (UpgradeStepper.CurrentStepIndex >= UpgradeStepper.Items.Count)
            UpgradeStepper.CurrentStepIndex = UpgradeStepper.Items.Count - 1;
        UpgradeStepper.IsCurrentStepError = !UpgradeStepper.IsCurrentStepError;
        UpdateStepperStatus();
    }

    private void OnStepperReset(object sender, RoutedEventArgs e)
    {
        UpgradeStepper.IsCurrentStepError = false;
        UpgradeStepper.CurrentStepIndex = 0;
        UpdateStepperStatus();
    }

    private void OnStepperStepClicked(object sender, SUIStepClickedEventArgs e)
    {
        UpgradeStepper.IsCurrentStepError = false;
        UpgradeStepper.CurrentStepIndex = e.Index;
        UpdateStepperStatus();
        if (e.Item is SUIStepItem step)
            ToastHost.Show($"已回看第 {e.StepNumber} 步「{step.Title}」。", InfoSeverity.Info, "SUIStepper");
    }

    private void UpdateStepperStatus()
    {
        var idx = UpgradeStepper.CurrentStepIndex;
        if (idx >= UpgradeStepper.Items.Count)
        {
            StepperStatusText.Text = "全部 5 个步骤已完成。";
            return;
        }
        var title = (UpgradeStepper.Items[idx] as SUIStepItem)?.Title ?? string.Empty;
        StepperStatusText.Text = UpgradeStepper.IsCurrentStepError
            ? $"第 {idx + 1} 步「{title}」异常，请检查后重试。"
            : $"当前处于第 {idx + 1} 步（{title}），已完成步骤支持点击回看。";
    }
}