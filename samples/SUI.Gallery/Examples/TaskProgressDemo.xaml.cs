using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;

namespace SuiExamples;

public partial class TaskProgressDemo : UserControl
{
    public TaskProgressDemo()
    {
        InitializeComponent();
        _timer.Tick += OnTick;
    }
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(160) };
    private void OnStart(object sender, RoutedEventArgs e)
    {
        if (_timer.IsEnabled) { Pause(); return; }
        if (TaskProgress.Value >= 100) TaskProgress.Value = 0;
        _timer.Start(); StartButton.Content = "暂停"; ProgressLabel.Text = $"正在同步 · {TaskProgress.Value:0}%";
    }
    private void OnTick(object? sender, EventArgs e)
    {
        TaskProgress.Value = Math.Min(100, TaskProgress.Value + 2);
        ProgressLabel.Text = $"正在同步 · {TaskProgress.Value:0}%";
        if (TaskProgress.Value >= 100) { _timer.Stop(); StartButton.Content = "重新开始"; ProgressLabel.Text = "同步完成 · 100%"; }
    }
    private void OnReset(object sender, RoutedEventArgs e) { _timer.Stop(); TaskProgress.Value = 0; StartButton.Content = "开始任务"; ProgressLabel.Text = "准备就绪 · 0%"; }
    private void Pause()
    {
        _timer.Stop();
        if (TaskProgress.Value is > 0 and < 100) { StartButton.Content = "继续任务"; ProgressLabel.Text = $"已暂停 · {TaskProgress.Value:0}%"; }
    }
    private void OnUnloaded(object sender, RoutedEventArgs e) => Pause();
}
