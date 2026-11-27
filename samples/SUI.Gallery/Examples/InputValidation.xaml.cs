using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SuiExamples;

public partial class InputValidation : UserControl
{
    public InputValidation()
    {
        InitializeComponent();
        DataContext = new WorkspaceSettings();
    }
    private void OnSave(object sender, RoutedEventArgs e)
    {
        ValidatedName.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        var invalid = Validation.GetHasError(ValidatedName);
        ValidationStatus.Text = invalid ? "请先修正名称，再保存设置。" : $"已保存「{ValidatedName.Text.Trim()}」 · 此操作只更新演示状态。";
        ValidationStatus.Foreground = (Brush)FindResource(invalid ? "SUI.Brush.Danger" : "SUI.Brush.Success");
    }
    private void OnValidationError(object sender, ValidationErrorEventArgs e)
    {
        if (ValidationStatus is null) return;
        ValidationStatus.Text = e.Action == ValidationErrorEventAction.Added ? e.Error.ErrorContent.ToString() : "名称有效，可以保存。";
        ValidationStatus.Foreground = (Brush)FindResource(e.Action == ValidationErrorEventAction.Added ? "SUI.Brush.Danger" : "SUI.Brush.TextSecondary");
    }
}

public sealed class WorkspaceSettings : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new();
    private double? _port = 8080;
    private TimeSpan? _scheduleTime = new(9, 30, 0);
    private DateTime? _effectiveAt = new(2026, 11, 27, 10, 0, 0);
    private string _protocolMode = "698.45";

    public string Name { get; set; } = "SUI 工作空间";

    public string[] ProtocolModes { get; } = ["请选择...", "698.45", "645-2007", "Modbus-RTU"];

    public string ProtocolMode
    {
        get => _protocolMode;
        set
        {
            if (_protocolMode == value) return;
            _protocolMode = value;
            OnPropertyChanged(nameof(ProtocolMode));
            SetError(nameof(ProtocolMode), value == "请选择..." ? "请选择有效的通信规约。" : null);
        }
    }

    public double? Port
    {
        get => _port;
        set
        {
            if (_port == value) return;
            _port = value;
            OnPropertyChanged(nameof(Port));
            ValidatePort();
        }
    }

    public TimeSpan? ScheduleTime
    {
        get => _scheduleTime;
        set
        {
            if (_scheduleTime == value) return;
            _scheduleTime = value;
            OnPropertyChanged(nameof(ScheduleTime));
            SetError(nameof(ScheduleTime), value is null ? "请选择执行时间。" : null);
        }
    }

    public DateTime? EffectiveAt
    {
        get => _effectiveAt;
        set
        {
            if (_effectiveAt == value) return;
            _effectiveAt = value;
            OnPropertyChanged(nameof(EffectiveAt));
            SetError(nameof(EffectiveAt), value is null ? "请选择生效日期时间。" : null);
        }
    }

    private void ValidatePort()
    {
        var invalid = _port is null || _port < 1 || _port > 65535;
        SetError(nameof(Port), invalid ? "端口号必须在 1 到 65535 之间。" : null);
    }

    private void SetError(string propertyName, string? message)
    {
        if (string.IsNullOrEmpty(message))
        {
            _errors.Remove(propertyName);
        }
        else
        {
            _errors[propertyName] = [message!];
        }
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    public bool HasErrors => _errors.Count > 0;
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? propertyName) =>
        propertyName is not null && _errors.TryGetValue(propertyName, out var list) ? list : Array.Empty<string>();

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class WorkspaceNameRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var name = value?.ToString()?.Trim() ?? "";
        return name.Length is >= 3 and <= 24 ? ValidationResult.ValidResult : new ValidationResult(false, "名称需要 3–24 个字符，且不能只包含空格。");
    }
}