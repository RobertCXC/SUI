using System.Windows;
using System.Windows.Controls;

namespace SuiExamples;

public partial class ButtonPlayground : UserControl
{
    private int _previewClicks;

    public ButtonPlayground()
    {
        InitializeComponent();
        UpdatePreview();
    }

    private void OnPreviewClick(object sender, RoutedEventArgs e) => PreviewResult.Text = $"已点击 {++_previewClicks} 次";
    private void OnPlaygroundChanged(object sender, RoutedEventArgs e) => UpdatePreview();

    private void UpdatePreview()
    {
        if (Preview is null || Variant is null || ButtonText is null || DisablePreview is null) return;
        var variant = (Variant.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Default";
        Preview.Content = ButtonText.Text;
        Preview.Style = (Style)FindResource(variant == "Default" ? typeof(Button) : $"SUI.Button.{variant}");
        Preview.IsEnabled = DisablePreview.IsChecked != true;
    }
}
