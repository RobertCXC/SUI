using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SUI.Gallery.Pages;
using SUI.Navigation;

namespace SUI.Gallery;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Func<UserControl>> _pages = new()
    {
        ["buttons"] = () => new ButtonsPage(),
        ["inputs"] = () => new InputsPage(),
        ["data"] = () => new DataPage(),
        ["containers"] = () => new ContainersPage(),
    };

    public MainWindow()
    {
        InitializeComponent();

        var items = new ObservableCollection<SUINavigationItem>
        {
            new() { Key = "buttons", Title = "按钮", IconGlyph = "\uE80F" },
            new() { Key = "inputs", Title = "输入", IconGlyph = "\uE70F" },
            new() { Key = "data", Title = "数据", IconGlyph = "\uE71D" },
            new() { Key = "containers", Title = "容器", IconGlyph = "\uE713" },
        };

        Navigation.ItemsSource = items;
        Navigation.SelectedItem = items[0];
    }

    private void OnNavigationSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 1
            && e.AddedItems[0] is SUINavigationItem item
            && _pages.TryGetValue(item.Key, out var create))
        {
            PageHost.Content = create();
        }
    }
}
