using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace SuiExamples;

public partial class MenusAndTooltips : UserControl
{
    public MenusAndTooltips()
    {
        InitializeComponent();
    }
    private void OnNew(object sender, ExecutedRoutedEventArgs e) { MenuResult.Text = "已新建演示项目 · Ctrl+N"; e.Handled = true; }
    private void OnSave(object sender, ExecutedRoutedEventArgs e) { MenuResult.Text = "已保存演示项目 · Ctrl+S"; e.Handled = true; }
    private void OnMenuAction(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem item) MenuResult.Text = $"已操作「{item.Header}」{(item.IsCheckable ? item.IsChecked ? " · 开启" : " · 关闭" : " · 仅演示")}";
        e.Handled = true;
    }
}
