using System.Windows.Controls;
using System.Windows;

namespace SuiExamples;

public partial class ButtonVariants : UserControl
{
    public ButtonVariants()
    {
        InitializeComponent();
    }
    private int _clicks;
    private void OnAction(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Button button)
        {
            var label = button.Content is string text && !string.IsNullOrWhiteSpace(text)
                ? text
                : button.ToolTip?.ToString() ?? "图标按钮";
            ActionResult.Text = $"已执行「{label}」 · 第 {++_clicks} 次操作（仅演示）";
        }
    }
}