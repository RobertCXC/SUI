using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SuiExamples;

public partial class CompactDataGridDemo : UserControl
{
    private readonly ObservableCollection<SchemeRow> _rows =
    [
        new() { Enabled = true, ObjectName = "设备时钟", Oad = "40000200", Operation = "读取", DelayMs = 100, ResultState = "Success", ResultText = "一致 · 通过" },
        new() { Enabled = true, ObjectName = "通信地址", Oad = "40010200", Operation = "设置", DelayMs = 150, ResultState = "Danger", ResultText = "不一致 · 预期 0001" },
        new() { Enabled = false, ObjectName = "冻结周期", Oad = "50040200", Operation = "读取", DelayMs = 200, ResultState = "Warning", ResultText = "超时等待重试" },
        new() { Enabled = true, ObjectName = "固件版本", Oad = "F0010200", Operation = "读取", DelayMs = 100, ResultState = "Info", ResultText = "读取中..." },
        new() { Enabled = false, ObjectName = "运行 APP", Oad = "FF020300", Operation = "动作", DelayMs = 300, ResultState = "Neutral", ResultText = "未执行" },
    ];

    public CompactDataGridDemo()
    {
        InitializeComponent();
        CompactGrid.ItemsSource = _rows;
    }

    private void OnRunRowClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: SchemeRow row })
        {
            row.ResultState = "Success";
            row.ResultText = $"已{row.Operation} · 一致";
            RowActionInfo.Text = $"已执行「{row.ObjectName} ({row.Oad})」{row.Operation}操作。";
        }
    }

    private void OnInspectRowClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: SchemeRow row })
        {
            RowActionInfo.Text = $"{row.ObjectName} · OAD={row.Oad} · 延时={row.DelayMs}ms · 状态={row.ResultText}";
        }
    }
}

public sealed class SchemeRow : INotifyPropertyChanged
{
    private bool _enabled;
    private string _objectName = string.Empty;
    private string _oad = string.Empty;
    private string _operation = "读取";
    private int _delayMs;
    private string _resultState = "Neutral";
    private string _resultText = string.Empty;

    public bool Enabled { get => _enabled; set => SetField(ref _enabled, value); }
    public string ObjectName { get => _objectName; set => SetField(ref _objectName, value); }
    public string Oad { get => _oad; set => SetField(ref _oad, value); }
    public string Operation { get => _operation; set => SetField(ref _operation, value); }
    public int DelayMs { get => _delayMs; set => SetField(ref _delayMs, value); }
    public string ResultState { get => _resultState; set => SetField(ref _resultState, value); }
    public string ResultText { get => _resultText; set => SetField(ref _resultText, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
