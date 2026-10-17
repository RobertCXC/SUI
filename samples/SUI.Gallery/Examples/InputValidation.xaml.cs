using System.Windows.Controls;
using System.Globalization;
using System.Windows;
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

public sealed class WorkspaceSettings
{
    public string Name { get; set; } = "SUI 工作空间";
}

public sealed class WorkspaceNameRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var name = value?.ToString()?.Trim() ?? "";
        return name.Length is >= 3 and <= 24 ? ValidationResult.ValidResult : new ValidationResult(false, "名称需要 3–24 个字符，且不能只包含空格。");
    }
}
