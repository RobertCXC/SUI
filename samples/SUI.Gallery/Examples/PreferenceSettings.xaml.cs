using System.Windows.Controls;
using System.Windows;

namespace SuiExamples;

public partial class PreferenceSettings : UserControl
{
    public PreferenceSettings()
    {
        InitializeComponent();
        UpdateSettings();
    }
    private void OnSettingsChanged(object sender, RoutedEventArgs e) => UpdateSettings();
    private void UpdateSettings()
    {
        if (SettingsSummary is null || Notifications is null || Comfortable is null || AutoSync is null) return;
        SettingsSummary.Text = $"当前设置 · 通知{(Notifications.IsChecked == true ? "开启" : "关闭")} / {(Comfortable.IsChecked == true ? "舒适" : "紧凑")}布局 / {(AutoSync.IsChecked == true ? "自动同步" : "手动同步")}";
    }
}
