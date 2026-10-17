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
        if (e.OriginalSource is Button button) ActionResult.Text = $"已执行「{button.Content}」 · 第 {++_clicks} 次操作（仅演示）";
    }
}
